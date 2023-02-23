using ModLib;

namespace CBXDecoder
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if RELEASE
            if (args.Length == 0)
            {
                Logger.Error("No arguments! You must drag a .CBX file over the executable file in order to convert to .WAV...");
                return;
            }
#else
            args = new string[1] {
                @"A:\Dimensions\EXTRACT\AUDIO\VO\DX\1WIZARDOFOZ\1WIZARDOFOZD\DX_WYLDSTYLE_THERESGOTTABEAWAY_GER.CBX"
            };
#endif
            Decoder.Decode(args[0]);

            Console.ReadKey();
        }
    }
}