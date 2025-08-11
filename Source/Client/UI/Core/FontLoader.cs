using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using Microsoft.Xna.Framework;

namespace CelestialLeague.Client.Resources
{
    public static class FontLoader
    {
        public static string GameDirectory = Directory.GetCurrentDirectory();
        public static string ModDirectory = Path.Combine(GameDirectory, "Mods", "Celestial League");
        private static string _contentFolder = "Content";

        private static readonly Dictionary<string, PixelFont> _loadedFonts = new Dictionary<string, PixelFont>();
        private static bool _initialized = false;

        public static string ContentDirectory => Path.Combine(ModDirectory, _contentFolder);
        public static string FontsDirectory => Path.Combine(ContentDirectory, "Fonts");

        public static void Initialize()
        {
            if (_initialized) return;

            try
            {
                if (!Directory.Exists(FontsDirectory))
                {
                    Directory.CreateDirectory(FontsDirectory);
                    Logger.Log(LogLevel.Info, "CelestialLeague", $"Created fonts directory: {FontsDirectory}");
                }

                Logger.Log(LogLevel.Info, "CelestialLeague", "FontLoader initialized successfully");
                _initialized = true;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Failed to initialize FontLoader: {ex.Message}");
            }
        }

        public static PixelFont LoadFont(string fontName)
        {
            if (!_initialized)
            {
                Initialize();
            }

            if (string.IsNullOrWhiteSpace(fontName))
            {
                Logger.Log(LogLevel.Warn, "CelestialLeague", "Cannot load font: fontName is null or empty");
                return null;
            }

            if (_loadedFonts.TryGetValue(fontName, out PixelFont cachedFont))
            {
                Logger.Log(LogLevel.Verbose, "CelestialLeague", $"Font '{fontName}' already loaded, returning cached version");
                return cachedFont;
            }

            try
            {
                PixelFont font = LoadFontFromDirectory(fontName);

                if (font != null)
                {
                    _loadedFonts[fontName] = font;
                    Logger.Log(LogLevel.Info, "CelestialLeague", $"Successfully loaded font: {fontName}");
                    return font;
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "CelestialLeague", $"Failed to load font: {fontName}");
                    return Dialog.Languages["english"].Font;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Exception while loading font '{fontName}': {ex.Message}");
                return Dialog.Languages["english"].Font;
            }
        }

        private static PixelFont LoadFontFromDirectory(string fontName)
        {
            string fontPath = Path.Combine(FontsDirectory, fontName);

            string fntFile = fontPath + ".fnt";
            if (File.Exists(fntFile))
            {
                return LoadBMFont(fontName, fntFile);
            }

            if (Directory.Exists(fontPath))
            {
                string subFntFile = Path.Combine(fontPath, fontName + ".fnt");
                if (File.Exists(subFntFile))
                {
                    return LoadBMFont(fontName, subFntFile);
                }

                var fntFiles = Directory.GetFiles(fontPath, "*.fnt");
                if (fntFiles.Length > 0)
                {
                    return LoadBMFont(fontName, fntFiles[0]);
                }
            }

            Logger.Log(LogLevel.Warn, "CelestialLeague", $"No font files found for: {fontName}");
            return null;
        }

        private static PixelFont LoadBMFont(string fontName, string fntPath)
        {
            try
            {
                Logger.Log(LogLevel.Verbose, "CelestialLeague", $"Loading BMFont: {fntPath}");

                var fontData = ParseBMFontFile(fntPath);
                if (fontData == null)
                {
                    Logger.Log(LogLevel.Error, "CelestialLeague", $"Failed to parse BMFont file: {fntPath}");
                    return null;
                }

                PixelFont pixelFont = new PixelFont(fontName);
                PixelFontSize fontSize = CreatePixelFontSizeFromBMFont(fontData, Path.GetDirectoryName(fntPath));
                if (fontSize != null)
                {
                    pixelFont.Sizes.Add(fontSize);
                    Logger.Log(LogLevel.Info, "CelestialLeague", $"Successfully created PixelFont from BMFont: {fontName}");
                    return pixelFont;
                }

                Logger.Log(LogLevel.Error, "CelestialLeague", $"Failed to create PixelFontSize from BMFont: {fontName}");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Exception loading BMFont '{fontName}': {ex.Message}");
                return null;
            }
        }

        private static BMFontData ParseBMFontFile(string fontPath)
        {
            try
            {
                // determine format by first line content
                string firstLine = File.ReadLines(fontPath).FirstOrDefault()?.Trim();

                if (firstLine != null && (firstLine.StartsWith("<?xml") || firstLine.StartsWith("<font")))
                {
                    return ParseBMFontXML(fontPath);
                }
                else
                {
                    return ParseBMFontText(fontPath);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Error parsing BMFont file: {ex.Message}");
                return null;
            }
        }

        private static BMFontData ParseBMFontXML(string xmlPath)
        {
            var fontData = new BMFontData();
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            var fontElement = xmlDoc.DocumentElement;
            if (fontElement?.Name != "font")
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", "Invalid BMFont XML: missing font root element");
                return null;
            }

            // parse info element
            var infoElement = fontElement.SelectSingleNode("info") as XmlElement;
            if (infoElement != null)
            {
                if (int.TryParse(infoElement.GetAttribute("size"), out int size))
                    fontData.Size = size;
            }

            // parse common element
            var commonElement = fontElement.SelectSingleNode("common") as XmlElement;
            if (commonElement != null)
            {
                if (int.TryParse(commonElement.GetAttribute("lineHeight"), out int lineHeight))
                    fontData.LineHeight = lineHeight;

                if (int.TryParse(commonElement.GetAttribute("scaleW"), out int scaleW))
                    fontData.TextureWidth = scaleW;

                if (int.TryParse(commonElement.GetAttribute("scaleH"), out int scaleH))
                    fontData.TextureHeight = scaleH;
            }

            // parse pages
            var pagesElement = fontElement.SelectSingleNode("pages") as XmlElement;
            if (pagesElement != null)
            {
                var pageElements = pagesElement.SelectNodes("page");
                foreach (XmlElement pageElement in pageElements)
                {
                    string fileName = pageElement.GetAttribute("file");
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        string texturePath = Path.Combine(Path.GetDirectoryName(xmlPath), fileName);
                        fontData.TexturePaths.Add(texturePath);
                    }
                }
            }

            // parse characters
            var charsElement = fontElement.SelectSingleNode("chars") as XmlElement;
            if (charsElement != null)
            {
                var charElements = charsElement.SelectNodes("char");
                foreach (XmlElement charElement in charElements)
                {
                    var charData = new BMCharData();

                    if (int.TryParse(charElement.GetAttribute("id"), out int id))
                        charData.Id = id;
                    if (int.TryParse(charElement.GetAttribute("x"), out int x))
                        charData.X = x;
                    if (int.TryParse(charElement.GetAttribute("y"), out int y))
                        charData.Y = y;
                    if (int.TryParse(charElement.GetAttribute("width"), out int width))
                        charData.Width = width;
                    if (int.TryParse(charElement.GetAttribute("height"), out int height))
                        charData.Height = height;
                    if (int.TryParse(charElement.GetAttribute("xoffset"), out int xOffset))
                        charData.XOffset = xOffset;
                    if (int.TryParse(charElement.GetAttribute("yoffset"), out int yOffset))
                        charData.YOffset = yOffset;
                    if (int.TryParse(charElement.GetAttribute("xadvance"), out int xAdvance))
                        charData.XAdvance = xAdvance;

                    fontData.Characters[charData.Id] = charData;
                }
            }

            // parse kerning pairs
            var kerningsElement = fontElement.SelectSingleNode("kernings") as XmlElement;
            if (kerningsElement != null)
            {
                var kerningElements = kerningsElement.SelectNodes("kerning");
                foreach (XmlElement kerningElement in kerningElements)
                {
                    if (int.TryParse(kerningElement.GetAttribute("first"), out int first) &&
                        int.TryParse(kerningElement.GetAttribute("second"), out int second) &&
                        int.TryParse(kerningElement.GetAttribute("amount"), out int amount))
                    {
                        var kerningPair = new BMKerningPair
                        {
                            First = first,
                            Second = second,
                            Amount = amount
                        };
                        fontData.KerningPairs.Add(kerningPair);
                    }
                }
            }

            Logger.Log(LogLevel.Info, "CelestialLeague",
                $"Parsed BMFont XML: Size={fontData.Size}, LineHeight={fontData.LineHeight}, " +
                $"Characters={fontData.Characters.Count}, Kerning={fontData.KerningPairs.Count}, Textures={fontData.TexturePaths.Count}");

            return fontData;
        }

        private static BMFontData ParseBMFontText(string fntPath)
        {
            var lines = File.ReadAllLines(fntPath);
            var fontData = new BMFontData();

            foreach (string line in lines)
            {
                var parts = line.Split(' ');
                if (parts.Length == 0) continue;

                switch (parts[0])
                {
                    case "info":
                        ParseInfoLine(parts, fontData);
                        break;
                    case "common":
                        ParseCommonLine(parts, fontData);
                        break;
                    case "page":
                        ParsePageLine(parts, fontData, Path.GetDirectoryName(fntPath));
                        break;
                    case "char":
                        ParseCharLine(parts, fontData);
                        break;
                    case "kerning":
                        ParseKerningLine(parts, fontData);
                        break;
                }
            }

            return fontData;
        }

        private static void ParseInfoLine(string[] parts, BMFontData fontData)
        {
            for (int i = 1; i < parts.Length; i++)
            {
                var keyValue = parts[i].Split('=');
                if (keyValue.Length != 2) continue;

                switch (keyValue[0])
                {
                    case "size":
                        if (int.TryParse(keyValue[1], out int size))
                            fontData.Size = Math.Abs(size);
                        break;
                }
            }
        }

        private static void ParseCommonLine(string[] parts, BMFontData fontData)
        {
            for (int i = 1; i < parts.Length; i++)
            {
                var keyValue = parts[i].Split('=');
                if (keyValue.Length != 2) continue;

                switch (keyValue[0])
                {
                    case "lineHeight":
                        if (int.TryParse(keyValue[1], out int lineHeight))
                            fontData.LineHeight = lineHeight;
                        break;
                    case "scaleW":
                        if (int.TryParse(keyValue[1], out int scaleW))
                            fontData.TextureWidth = scaleW;
                        break;
                    case "scaleH":
                        if (int.TryParse(keyValue[1], out int scaleH))
                            fontData.TextureHeight = scaleH;
                        break;
                }
            }
        }

        private static void ParsePageLine(string[] parts, BMFontData fontData, string basePath)
        {
            string fileName = null;
            for (int i = 1; i < parts.Length; i++)
            {
                var keyValue = parts[i].Split('=');
                if (keyValue.Length != 2) continue;

                if (keyValue[0] == "file")
                {
                    fileName = keyValue[1].Trim('"');
                    break;
                }
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                string texturePath = Path.Combine(basePath, fileName);
                if (File.Exists(texturePath))
                {
                    fontData.TexturePaths.Add(texturePath);
                }
            }
        }

        private static void ParseCharLine(string[] parts, BMFontData fontData)
        {
            var charData = new BMCharData();

            for (int i = 1; i < parts.Length; i++)
            {
                var keyValue = parts[i].Split('=');
                if (keyValue.Length != 2) continue;

                switch (keyValue[0])
                {
                    case "id":
                        if (int.TryParse(keyValue[1], out int id))
                            charData.Id = id;
                        break;
                    case "x":
                        if (int.TryParse(keyValue[1], out int x))
                            charData.X = x;
                        break;
                    case "y":
                        if (int.TryParse(keyValue[1], out int y))
                            charData.Y = y;
                        break;
                    case "width":
                        if (int.TryParse(keyValue[1], out int width))
                            charData.Width = width;
                        break;
                    case "height":
                        if (int.TryParse(keyValue[1], out int height))
                            charData.Height = height;
                        break;
                    case "xoffset":
                        if (int.TryParse(keyValue[1], out int xOffset))
                            charData.XOffset = xOffset;
                        break;
                    case "yoffset":
                        if (int.TryParse(keyValue[1], out int yOffset))
                            charData.YOffset = yOffset;
                        break;
                    case "xadvance":
                        if (int.TryParse(keyValue[1], out int xAdvance))
                            charData.XAdvance = xAdvance;
                        break;
                }
            }

            fontData.Characters[charData.Id] = charData;
        }

        private static void ParseKerningLine(string[] parts, BMFontData fontData)
        {
            var kerningPair = new BMKerningPair();

            for (int i = 1; i < parts.Length; i++)
            {
                var keyValue = parts[i].Split('=');
                if (keyValue.Length != 2) continue;

                switch (keyValue[0])
                {
                    case "first":
                        if (int.TryParse(keyValue[1], out int first))
                            kerningPair.First = first;
                        break;
                    case "second":
                        if (int.TryParse(keyValue[1], out int second))
                            kerningPair.Second = second;
                        break;
                    case "amount":
                        if (int.TryParse(keyValue[1], out int amount))
                            kerningPair.Amount = amount;
                        break;
                }
            }

            fontData.KerningPairs.Add(kerningPair);
        }

        private static PixelFontSize CreatePixelFontSizeFromBMFont(BMFontData fontData, string basePath)
        {
            try
            {
                if (fontData.TexturePaths.Count == 0)
                {
                    Logger.Log(LogLevel.Error, "CelestialLeague", "No texture paths found in BMFont data");
                    return null;
                }

                string texturePath = fontData.TexturePaths[0];
                if (!File.Exists(texturePath))
                {
                    Logger.Log(LogLevel.Error, "CelestialLeague", $"Texture file not found: {texturePath}");
                    return null;
                }

                // load texture directly from file (BMFont textures are not in Celeste's atlases)
                MTexture atlasTexture;
                try
                {
                    Logger.Log(LogLevel.Info, "CelestialLeague", $"Loading texture from: {texturePath}");

                    using (var fileStream = File.OpenRead(texturePath))
                    {
                        var texture2D = Texture2D.FromStream(Engine.Graphics.GraphicsDevice, fileStream);
                        var virtualTexture = VirtualContent.CreateTexture(Path.GetFileNameWithoutExtension(texturePath), texture2D.Width, texture2D.Height, Color.Transparent);
                        virtualTexture.Texture_Safe = texture2D;
                        atlasTexture = new MTexture(virtualTexture);
                    }

                    Logger.Log(LogLevel.Info, "CelestialLeague", $"Successfully loaded texture: {Path.GetFileName(texturePath)}");
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "CelestialLeague", $"Failed to load texture: {ex.Message}");
                    return null;
                }

                PixelFontSize pixelFontSize = new PixelFontSize()
                {
                    Size = fontData.Size,
                    LineHeight = fontData.LineHeight,
                    Textures = new List<MTexture> { atlasTexture },
                    Characters = new Dictionary<int, PixelFontCharacter>()
                };

                Logger.Log(LogLevel.Info, "CelestialLeague", $"Created PixelFontSize: Size={pixelFontSize.Size}, LineHeight={pixelFontSize.LineHeight}, Textures={pixelFontSize.Textures.Count}");

                // convert BMFont characters to PixelFontCharacters
                foreach (var kvp in fontData.Characters)
                {
                    var bmChar = kvp.Value;

                    try
                    {
                        MTexture characterTexture;
                        if (bmChar.Width > 0 && bmChar.Height > 0)
                        {
                            characterTexture = atlasTexture.GetSubtexture(
                                bmChar.X, bmChar.Y, bmChar.Width, bmChar.Height
                            );
                        }
                        else
                        {
                            characterTexture = atlasTexture.GetSubtexture(0, 0, 1, 1);
                        }

                        if (characterTexture == null)
                        {
                            Logger.Log(LogLevel.Error, "CelestialLeague", $"Failed to create subtexture for character {bmChar.Id}");
                            continue;
                        }

                        var kerning = new Dictionary<int, int>();
                        foreach (var kerningPair in fontData.KerningPairs)
                        {
                            if (kerningPair.First == bmChar.Id)
                            {
                                kerning[kerningPair.Second] = kerningPair.Amount;
                            }
                        }

                        XmlDocument xmlDoc = new XmlDocument();
                        XmlElement charElement = xmlDoc.CreateElement("char");
                        charElement.SetAttribute("id", bmChar.Id.ToString());
                        charElement.SetAttribute("x", bmChar.X.ToString());
                        charElement.SetAttribute("y", bmChar.Y.ToString());
                        charElement.SetAttribute("width", bmChar.Width.ToString());
                        charElement.SetAttribute("height", bmChar.Height.ToString());
                        charElement.SetAttribute("xoffset", bmChar.XOffset.ToString());
                        charElement.SetAttribute("yoffset", bmChar.YOffset.ToString());
                        charElement.SetAttribute("xadvance", bmChar.XAdvance.ToString());

                        var character = new PixelFontCharacter(bmChar.Id, characterTexture, charElement);
                        character.Kerning = kerning;

                        if (character == null)
                        {
                            Logger.Log(LogLevel.Error, "CelestialLeague", $"Failed to create PixelFontCharacter for character {bmChar.Id}");
                            continue;
                        }

                        pixelFontSize.Characters[bmChar.Id] = character;
                    }
                    catch (Exception charEx)
                    {
                        Logger.Log(LogLevel.Error, "CelestialLeague", $"Exception creating character {bmChar.Id}: {charEx.Message}");
                        Logger.Log(LogLevel.Error, "CelestialLeague", $"Character details - X:{bmChar.X}, Y:{bmChar.Y}, W:{bmChar.Width}, H:{bmChar.Height}");
                    }
                }

                Logger.Log(LogLevel.Info, "CelestialLeague",
                    $"Successfully created PixelFontSize: Size={pixelFontSize.Size}, " +
                    $"LineHeight={pixelFontSize.LineHeight}, Characters={pixelFontSize.Characters.Count}");

                return pixelFontSize;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "CelestialLeague", $"Error creating PixelFontSize from BMFont: {ex.Message}");
                return null;
            }
        }
    }

    internal class BMFontData
    {
        public int Size { get; set; }
        public int LineHeight { get; set; }
        public int TextureWidth { get; set; }
        public int TextureHeight { get; set; }
        public List<string> TexturePaths { get; set; } = new List<string>();
        public Dictionary<int, BMCharData> Characters { get; set; } = new Dictionary<int, BMCharData>();
        public List<BMKerningPair> KerningPairs { get; set; } = new List<BMKerningPair>();
    }

    internal class BMCharData
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int XOffset { get; set; }
        public int YOffset { get; set; }
        public int XAdvance { get; set; }
    }

    internal class BMKerningPair
    {
        public int First { get; set; }
        public int Second { get; set; }
        public int Amount { get; set; }
    }
}