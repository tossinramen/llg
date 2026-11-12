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
        void OnPreprocessTexture()
        {
            if (!assetPath.Contains("/Art/")) return;

            var importer = (TextureImporter)assetImporter;
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
