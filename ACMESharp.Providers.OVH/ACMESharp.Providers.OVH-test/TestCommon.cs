using ACMESharp.ACME;

namespace ACMESharp.Providers.OVH_test
{
    static class TestCommon
    {
        public static readonly Challenge DNS_CHALLENGE = new DnsChallenge("", new DnsChallengeAnswer());
        public static readonly Challenge HTTP_CHALLENGE = new HttpChallenge("", new HttpChallengeAnswer());
        public static readonly Challenge TLS_SNI_CHALLENGE = new TlsSniChallenge("", new TlsSniChallengeAnswer());
        public static readonly Challenge FAKE_CHALLENGE = new FakeChallenge(new FakeChallengeAnswer());

        class FakeChallengeAnswer : ChallengeAnswer
        {

        }

        class FakeChallenge : Challenge
        {
            public FakeChallenge(ChallengeAnswer answer)
                    : base(ChallengeTypeKind.OTHER, "", answer)
            { }
        }
    }
}
