namespace TeensyRom.Core.Serial
{
    public static class TRTokenFinder
    {
        public static List<TeensyToken> FindTRTokens(this byte[] input)
        {
            var foundTokens = new List<TeensyToken>();
            var inputLength = input.Length;

            foreach (var token in TeensyToken.List)
            {
                byte[] tokenBytes = BitConverter.GetBytes(token.Value);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(tokenBytes);
                }
                for (int i = 0; i <= inputLength - tokenBytes.Length; i++)
                {
                    if (input.Skip(i).Take(tokenBytes.Length).SequenceEqual(tokenBytes))
                    {
                        foundTokens.Add(token);
                    }
                }
            }
            return foundTokens;
        }
    }
}
