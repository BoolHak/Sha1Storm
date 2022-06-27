using System.Buffers;

namespace API.Utils
{
    public class Sha1Generator
    {
        public const short Sha1Length = 20;
        public static void Generate(out byte[] buffer, int nbHashes)
        {
            if (nbHashes < 1)
            {
                buffer = Array.Empty<byte>();
                return;
            }

            buffer = ArrayPool<byte>.Shared.Rent(nbHashes*Sha1Length);

            using var sha1 = System.Security.Cryptography.SHA1.Create();

            for(int i = 0; i < nbHashes; i++)
            {
                var payload = sha1.ComputeHash(Guid.NewGuid().ToByteArray());
                payload.CopyTo(buffer, i*Sha1Length);
            }

        }


    }
}
