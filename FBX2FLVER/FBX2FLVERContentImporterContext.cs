extern alias PIPE;
using PIPE::Microsoft.Xna.Framework.Content.Pipeline;
using System;

namespace FBX2FLVER
{
    internal class FBX2FLVERContentImporterContext : ContentImporterContext
    {
        public override void AddDependency(string filename)
        {
            throw new NotImplementedException();
        }

        ContentBuildLogger _logger = new FBX2FLVERContentBuildLogger();

        public override ContentBuildLogger Logger => _logger;
        public override string OutputDirectory => "ContentImporterContext_out";
        public override string IntermediateDirectory => "ContentImporterContext_inter";
    }
}
