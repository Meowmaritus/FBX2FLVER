extern alias PIPE;
using PIPE::Microsoft.Xna.Framework.Content.Pipeline;
using System;

namespace FBX2FLVER
{
    public class FBX2FLVERContentBuildLogger : ContentBuildLogger
    {
        public override void LogMessage(string message, params object[] messageArgs)
        {
            Console.WriteLine("XNA CONTENT BUILD LOG --> " + string.Format(message, messageArgs));
        }

        public override void LogImportantMessage(string message, params object[] messageArgs)
        {
            Console.WriteLine("XNA CONTENT BUILD LOG (\"IMPORTANT\") --> " + string.Format(message, messageArgs));
        }

        public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs)
        {
            Console.WriteLine("XNA CONTENT BUILD LOG (WARNING) --> " + string.Format(message, messageArgs));
        }
    }
}
