using UnityEditor;
using UnityEngine;

namespace LLG.EditorTools
{
    /// <summary>
    /// Every image dropped anywhere under an "Art" folder is automatically imported as a
    /// crisp pixel-art sprite (no blur, no compression). You never need to touch import settings.
    /// </summary>
    public class PixelArtImporter : AssetPostprocessor
    {
        public override uint GetVersion() => 3;

        /// <summary>Pixel fonts under a "Fonts" folder are drawn without smoothing.</summary>
        void OnPreprocessAsset()
        {
            if (assetImporter is TrueTypeFontImporter font && assetPath.Contains("/Fonts/"))
                font.fontRenderingMode = FontRenderingMode.HintedRaster;
        }

        void OnPreprocessTexture()
        {
            if (!assetPath.Contains("/Art/")) return;

            var importer = (TextureImporter)assetImporter;
            if (assetPath.Contains("/Cursor/"))
            {
                importer.textureType = TextureImporterType.Cursor;     // mouse pointer images
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                return;
            }
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
        }
    }
}
