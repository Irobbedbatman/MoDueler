using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoDueler.Audio {

    /// <summary>
    /// Audio controller to play a single BGM <see cref="AudioStreamPlayer"/> and upto a set amount in MAX_SFX_PLAYERS of SFX <see cref="AudioStreamPlayer"/>s.
    /// </summary>
    [MoonSharp.Interpreter.MoonSharpUserData]
    public class AudioController : Node {

        /// <summary>
        /// The index for the bus gotten from <see cref="AudioServer.GetBusName(int)"/>.
        /// </summary>
        public const int SFX_BUS = 2;
        /// <summary>
        /// The maximum amount of <see cref="AudioStreamPlayer"/> created for use of sound effects.
        /// TODO: Provide a more accurate number for a everyday player and/or make it variable instead of constant.
        /// </summary>
        public const int MAX_SFX_PLAYERS = 8;

        /// <summary>
        /// The accessor for the single <see cref="AudioController"/>.
        /// </summary>
        public static AudioController Instance { get; private set; }

        /// <summary>
        /// The <see cref="AudioStreamPlayer"/> used for background music.
        /// </summary>
        private AudioStreamPlayer BGM;
        /// <summary>
        /// The <see cref="Node"/> that contains all the created <see cref="AudioStreamPlayer"/>s.
        /// </summary>
        private Node SFXPool;
        /// <summary>
        /// The <see cref="Queue{T}"/> of <see cref="AudioStreamPlayer"/>s not currently in use.
        /// </summary>
        private readonly Queue<AudioStreamPlayer> PooledAudioStreams = new Queue<AudioStreamPlayer>();

        public override void _Ready() {
            // Only allow 1 instance at any time.
            if (Instance != null)
                return;
            Instance = this;
            // Get the sub nodes from the audio controller.
            BGM = GetNode<AudioStreamPlayer>("BGM");
            SFXPool = GetNode("SFXPool");
            // Esnure we have enough buses for creating a new one.
            AudioServer.BusCount++;
            // Setup the bus for SFX.
            AudioServer.AddBus(SFX_BUS);
            AudioServer.SetBusName(SFX_BUS, "SFX");
            // Set Volume to not rip my ears.
            // TODO: Set correct value.
            SetVolume(1f);
        }

        /// <summary>
        /// Plays a <see cref="AudioStream"/> on the <see cref="BGM"/> Player.
        /// <para>Supports looping and a loop offset can aslo be provided.</para>
        /// </summary>
        /// <param name="key">The file key to play.</param>
        /// <param name="loop">If the track should loop.</param>
        /// <param name="loopOffset">The time in seconds thre track restarts after finishing when looping.</param>
        public void PlayBGM(string key, bool loop = false, float loopOffset = 0) => PlayAudioOnPlayer(BGM, key, loop, loopOffset);

        /// <summary>
        /// Plays a <see cref="AudioStream"/> on a <see cref="AudioStreamPlayer"/> retrieved from <see cref="GetSFXStream"/>.
        /// <para>Doesn't support looping.</para>
        /// </summary>
        /// <param name="key">The file key to play.</param>
        public void PlaySFX(string key) => PlayAudioOnPlayer(GetSFXStream(), key);

        /// <summary>
        /// Set the volume of the BGM and SFX to 0.
        /// </summary>
        public void Mute() => SetVolume(0);

        /// <summary>
        /// Sets volume to a linear value. 
        /// </summary>
        /// <param name="percent">Not really a percentage but effectially a rate that works.</param>
        public void SetVolume(float percent) => SetDbVolume(GD.Linear2Db(percent));

        /// <summary>
        /// Sets the decibel volume of the BGM and SFX to the provided value.
        /// </summary>
        public void SetDbVolume(float volume) {
            //TODO: Seperate Volume for BGM and SFX.
            BGM.VolumeDb = volume;
            AudioServer.SetBusVolumeDb(SFX_BUS, volume);
        }

        /// <summary>
        /// The signal response for when a <see cref="AudioStreamPlayer"/> has finished so it can be added to <see cref="PooledAudioStreams"/> for reuse.
        /// </summary>
        /// <param name="player">The player that provided the signal.</param>
        private void PlayerFinished(AudioStreamPlayer player) => PooledAudioStreams.Enqueue(player);

        /// <summary>
        /// Get's a <see cref="AudioStreamPlayer"/> from the <see cref="PooledAudioStreams"/> or creates a new one.
        /// <para>Can't create a new <see cref="AudioStreamPlayer"/> if we have reached the <see cref="MAX_SFX_PLAYERS"/>.</para>
        /// </summary>
        private AudioStreamPlayer GetSFXStream() {
            // If we can use a finished player we use it.
            if (PooledAudioStreams.Count > 0)
                return PooledAudioStreams.Dequeue();
            // Create only so many SFX to reserve memory and processing power.
            else if (SFXPool.GetChildCount() >= MAX_SFX_PLAYERS)
                return null;
            // Create a new player.
            AudioStreamPlayer player = new AudioStreamPlayer {
                Bus = AudioServer.GetBusName(SFX_BUS), // Set the bus.
                Name = "SFX" + SFXPool.GetChildCount() // Set the name for readability.
            };
            // Add it to the tree.
            SFXPool.AddChild(player);
            // Connect the finished signal to Player Finished so that we can tell when to return it to the pool.
            player.Connect("finished", this, nameof(PlayerFinished), new Godot.Collections.Array { player });
            return player;
        }

        /// <summary>
        /// Finds and loads a <see cref="File"/> that contains audio information with the <paramref name="key"/> and plays it on the provided <paramref name="player"/>.
        /// </summary>
        /// <param name="player">The <see cref="AudioStreamPlayer"/> to play on.</param>
        /// <param name="key">The file to load that contans <see cref="AudioStream"/> data.</param>
        /// <param name="loop">Wether the <see cref="AudioStream"/> should loop.</param>
        /// <param name="loopOffset">The time in seconds thre track restarts after finishing when looping.</param>
        private void PlayAudioOnPlayer(AudioStreamPlayer player, string key, bool loop = false, float loopOffset = 0) {
            // If no player is provided we can't play at all.
            if (player == null)
                return;

            // Find the file.
            string file = Resources.ResourceFiles.FindFile(key);

            // Get the extension so we know which loader to use.
            string extension = System.IO.Path.GetExtension(file);

            // Open the file and read all it's bytes.
            var f = new File();
            f.Open(file, File.ModeFlags.Read);
            byte[] bytes = f.GetBuffer((int)f.GetLen());
            f.Close();

            // TODO: Cache sounds.

            // Create a stream to provide.
            AudioStream stream;
            // Load from extensions.
            if (extension == ".mp3")
                stream = LoadMP3(bytes, loop, loopOffset);
            else if (extension == ".wav")
                stream = LoadWav(bytes);
            else if (extension == ".ogg") {
                stream = LoadOgg(bytes, loop, loopOffset);
            }
            else {
                // No loader provided so we can't play the file.
                return;
            }
            
            // Set the player stream to the loaded stream.
            player.Stream = stream;
            // Play the stream.
            player.Play();
        }

        /// <summary>
        /// Creates a <see cref="AudioStreamMP3"/> from the provided bytes,
        /// </summary>
        private AudioStream LoadMP3(byte[] bytes, bool loop = false, float loopOffset = 0) => new AudioStreamMP3() { 
            Data = bytes,
            Loop = loop,
            LoopOffset = loopOffset
        };

        /// <summary>
        /// Creates a <see cref="AudioStreamOGGVorbis"/> from the provided bytes.
        /// </summary>
        private AudioStream LoadOgg(byte[] bytes, bool loop = false, float loopOffset = 0) => new AudioStreamOGGVorbis {
            Loop = loop,
            LoopOffset = loopOffset,
            Data = bytes
        };

        /// <summary>
        /// Read GDScriptAudioImport.gd for information; I just converted it to C# and removed the simple documentation and printing.
        /// </summary>
        private AudioStream LoadWav(byte[] bytes) {
            var stream = new AudioStreamSample();
            for (int i = 0; i < 100; ++i) {
                StringBuilder strBldr = new StringBuilder();
                strBldr.Append((char)(bytes[i]));
                strBldr.Append((char)(bytes[i+1]));
                strBldr.Append((char)(bytes[i+2]));
                strBldr.Append((char)(bytes[i+3]));
                string code = strBldr.ToString();
                if (code == "fmt ") {
                    //var formatsubchunksize = bytes[i + 4] + (bytes[i + 5] << 8) + (bytes[i + 6] << 16) + (bytes[i + 7] << 32);
                    var fsc0 = i + 8;
                    var format_code = bytes[fsc0] + (bytes[fsc0 + 1] << 8);
                    AudioStreamSample.FormatEnum format; //default;
                    switch (format_code) {
                        case 0:
                            format = AudioStreamSample.FormatEnum.Format8Bits;
                            break;
                        case 1:
                            format = AudioStreamSample.FormatEnum.Format16Bits;
                            break;
                        case 2:
                            format = AudioStreamSample.FormatEnum.ImaAdpcm;
                            break;
                        default:
                            GD.Print("Wave Format Incorrect.");
                            return null;
                    }
                    stream.Format = format;

                    var channel_num = bytes[fsc0 + 2] + (bytes[fsc0 + 3] << 8);
                    if (channel_num == 2)
                        stream.Stereo = true;

                    /* Can use this to trim off seconds. 
                    var byte_rate = bytes[fsc0 + 8] + (bytes[fsc0 + 9] << 8) + (bytes[fsc0 + 10] << 16) + (bytes[fsc0 + 11] << 32);
                    GD.Print("Byte rate: " + byte_rate);
                    */

                    var sample_rate = bytes[fsc0 + 4] + (bytes[fsc0 + 5] << 8) + (bytes[fsc0 + 6] << 16) + (bytes[fsc0 + 7] << 32);
                    stream.MixRate = sample_rate;
                }
                else if (code == "data") {
                    var audio_data_size = bytes[i + 4] + (bytes[i + 5] << 8) + (bytes[i + 6] << 16) + (bytes[i + 7] << 32);
                    var data_entry_point = (i + 8);
                    //TODO: To trim the annoying click i remove an extra 127 bytes. This line was originaly audio_data_size - 1. There could be an exact amount.
                    stream.Data = bytes.ToList().GetRange(data_entry_point, data_entry_point + audio_data_size - 128).ToArray();
                }
            }

            return stream;
        }




    }
}
