namespace Giants.Launcher
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public static class RenderInfoExtensions
    {
        /// <summary>
        /// Disambiguates renderers with the same name by adding the file name.
        /// </summary>
        public static IList<RendererInfo> Disambiguate(this IList<RendererInfo> rendererInfos)
        {
            foreach (var group in rendererInfos.GroupBy(x => x.Name))
            {
                if (group.Count() > 1)
                {
                    foreach (var rendererInfo in group)
                    {
                        rendererInfo.Name = $"{rendererInfo.Name} ({Path.GetFileName(rendererInfo.FilePath)})";
                    }
                }
            }

            return rendererInfos;
        }
    }
}
