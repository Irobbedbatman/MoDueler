using System;

namespace MoDueler {

    /// <summary>
    /// Singleton accessor for Player Profile Settings for use in lua without direct access.
    /// <para>Not all features will be accessible and most will be read only.</para>
    /// </summary>
    [MoonSharp.Interpreter.MoonSharpUserData]
    public sealed class ProfileAccessor {

        private static readonly Lazy<ProfileAccessor> _profile = new Lazy<ProfileAccessor>(() => new ProfileAccessor());
        public static ProfileAccessor Profile => _profile.Value;
        private ProfileAccessor () {}

        public string UserId => PlayerProfile.UserId;


    }
}
