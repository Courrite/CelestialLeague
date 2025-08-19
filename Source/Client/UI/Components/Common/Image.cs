using CelestialLeague.Client.UI.Types;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace CelestialLeague.Client.UI.Components
{
    public class Image : Panel
    {
        private MTexture mTexture;
        private SpriteSheetConfig spriteSheetConfig;
        private float animationTimer = 0f;
        private int currentFrame = 0;
        private bool animationPlaying = true;
        private bool sizeDirty = true;

        public ImageType ImageType { get; set; } = ImageType.Static;
        public ScaleType ScaleType { get; set; } = ScaleType.Fit;

        public MTexture Texture
        {
            get => mTexture;
            set
            {
                if (mTexture != value)
                {
                    mTexture = value;
                    sizeDirty = true;
                    InvalidateLayout();
                }
            }
        }

        public SpriteSheetConfig SpriteSheet
        {
            get => spriteSheetConfig;
            set
            {
                spriteSheetConfig = value;
                ImageType = ImageType.SpriteSheet;
                animationTimer = 0f;
                currentFrame = 0;
                sizeDirty = true;
                InvalidateLayout();
            }
        }

        public float AnimationSpeed { get; set; } = 1.0f;
        public bool AnimationLooping { get; set; } = true;
        public Color Tint { get; set; } = Color.White;
        public float Transparency { get; set; } = 0.0f;
        public bool AutoSize { get; set; } = false;
        public Rectangle? CustomSourceRectangle { get; set; } = null;

        public bool IsAnimationPlaying
        {
            get => animationPlaying;
            set => animationPlaying = value;
        }

        public int CurrentFrame
        {
            get => currentFrame;
            set
            {
                if (ImageType == ImageType.SpriteSheet && spriteSheetConfig.TotalFrames > 0)
                {
                    currentFrame = Math.Max(0, Math.Min(value, spriteSheetConfig.TotalFrames - 1));
                }
            }
        }

        public Image() : base()
        {
            CanReceiveFocus = false;
            BackgroundColor = Color.Transparent;
        }

        public Image(MTexture texture) : this()
        {
            Texture = texture;
            ImageType = ImageType.Static;
        }

        public Image(MTexture texture, SpriteSheetConfig spriteSheetConfig) : this()
        {
            Texture = texture;
            SpriteSheet = spriteSheetConfig;
        }

        protected override void UpdateSelf(InterfaceManager ui)
        {
            base.UpdateSelf(ui);

            if (sizeDirty)
            {
                RecalculateSize();
                sizeDirty = false;
            }

            if (ImageType == ImageType.SpriteSheet && mTexture != null && animationPlaying && spriteSheetConfig.TotalFrames > 1)
            {
                UpdateSpriteSheetAnimation(Engine.DeltaTime);
            }
        }

        protected override void RenderSelf(InterfaceManager ui)
        {
            base.RenderSelf(ui);

            var bounds = GetWorldBounds();
            Color renderColor = Tint * (1.0f - Transparency);

            if (mTexture != null)
            {
                if (ImageType == ImageType.Static)
                {
                    RenderStaticTexture(bounds, renderColor);
                }
                else if (ImageType == ImageType.SpriteSheet)
                {
                    RenderSpriteSheet(bounds, renderColor);
                }
            }
        }

        private void RecalculateSize()
        {
            if (AutoSize && mTexture != null && !Layout.Size.X.IsSet && !Layout.Size.Y.IsSet)
            {
                Layout.AbsoluteSize = new Vector2(mTexture.Width, mTexture.Height);
            }
        }

        private void UpdateSpriteSheetAnimation(float deltaTime)
        {
            if (spriteSheetConfig.TotalFrames <= 1) return;

            animationTimer += deltaTime * AnimationSpeed;
            if (animationTimer >= spriteSheetConfig.FrameDuration)
            {
                animationTimer -= spriteSheetConfig.FrameDuration;
                currentFrame++;
                if (currentFrame >= spriteSheetConfig.TotalFrames)
                {
                    if (AnimationLooping)
                    {
                        currentFrame = 0;
                    }
                    else
                    {
                        currentFrame = spriteSheetConfig.TotalFrames - 1;
                        animationPlaying = false;
                    }
                }
            }
        }

        private MTexture GetCurrentFrameTexture()
        {
            if (ImageType != ImageType.SpriteSheet || mTexture == null)
            {
                return mTexture;
            }

            int frameX = currentFrame % spriteSheetConfig.FramesPerRow;
            int frameY = currentFrame / spriteSheetConfig.FramesPerRow;

            var frameRect = new Rectangle(
                spriteSheetConfig.StartOffset.X + (frameX * spriteSheetConfig.FrameWidth),
                spriteSheetConfig.StartOffset.Y + (frameY * spriteSheetConfig.FrameHeight),
                spriteSheetConfig.FrameWidth,
                spriteSheetConfig.FrameHeight
            );

            return mTexture.GetSubtexture(frameRect);
        }

        private void RenderStaticTexture(Rectangle bounds, Color color)
        {
            var sourceRect = CustomSourceRectangle ?? mTexture.ClipRect;
            
            switch (ScaleType)
            {
                case ScaleType.Tile:
                    RenderTiled(bounds, sourceRect, color);
                    break;
                case ScaleType.Stretch:
                case ScaleType.Fit:
                case ScaleType.Crop:
                    var destRect = CalculateDestinationRectangle(bounds, new Vector2(sourceRect.Width, sourceRect.Height));
                    mTexture.Draw(new Vector2(destRect.X, destRect.Y), Vector2.Zero, color, new Vector2((float)destRect.Width / sourceRect.Width, (float)destRect.Height / sourceRect.Height));
                    break;
            }
        }

        private void RenderSpriteSheet(Rectangle bounds, Color color)
        {
            MTexture currentFrameTexture = GetCurrentFrameTexture();

            if (currentFrameTexture != null)
            {
                var destRect = CalculateDestinationRectangle(bounds, new Vector2(currentFrameTexture.Width, currentFrameTexture.Height));
                currentFrameTexture.Draw(new Vector2(destRect.X, destRect.Y), Vector2.Zero, color, new Vector2((float)destRect.Width / currentFrameTexture.Width, (float)destRect.Height / currentFrameTexture.Height));
            }
        }

        private void RenderTiled(Rectangle bounds, Rectangle sourceRect, Color color)
        {
            var textureSize = new Vector2(sourceRect.Width, sourceRect.Height);
            for (float y = bounds.Y; y < bounds.Bottom; y += textureSize.Y)
            {
                for (float x = bounds.X; x < bounds.Right; x += textureSize.X)
                {
                    float tileWidth = Math.Min(textureSize.X, bounds.Right - x);
                    float tileHeight = Math.Min(textureSize.Y, bounds.Bottom - y);

                    var sourceClipRect = new Rectangle(
                        sourceRect.X,
                        sourceRect.Y,
                        (int)tileWidth,
                        (int)tileHeight
                    );

                    MTexture clippedTile = mTexture.GetSubtexture(sourceClipRect);

                    clippedTile.Draw(new Vector2(x, y), Vector2.Zero, color, Vector2.One);
                }
            }
        }

        private Rectangle CalculateDestinationRectangle(Rectangle bounds, Vector2 textureSize)
        {
            float scale = 1.0f;
            Rectangle destRect = bounds;

            if (ScaleType == ScaleType.Fit)
            {
                float scaleX = bounds.Width / textureSize.X;
                float scaleY = bounds.Height / textureSize.Y;
                scale = Math.Min(scaleX, scaleY);
            }
            else if (ScaleType == ScaleType.Crop)
            {
                float scaleX = bounds.Width / textureSize.X;
                float scaleY = bounds.Height / textureSize.Y;
                scale = Math.Max(scaleX, scaleY);
            }

            destRect.Width = (int)(textureSize.X * scale);
            destRect.Height = (int)(textureSize.Y * scale);
            destRect.X = bounds.X + (bounds.Width - destRect.Width) / 2;
            destRect.Y = bounds.Y + (bounds.Height - destRect.Height) / 2;

            return destRect;
        }

        public void PlayAnimation()
        {
            if (ImageType == ImageType.SpriteSheet)
            {
                animationPlaying = true;
            }
        }

        public void PauseAnimation()
        {
            if (ImageType == ImageType.SpriteSheet)
            {
                animationPlaying = false;
            }
        }

        public void StopAnimation()
        {
            if (ImageType == ImageType.SpriteSheet)
            {
                animationPlaying = false;
                currentFrame = 0;
                animationTimer = 0f;
            }
        }

        public void RestartAnimation()
        {
            if (ImageType == ImageType.SpriteSheet)
            {
                currentFrame = 0;
                animationTimer = 0f;
                animationPlaying = true;
            }
        }
        
        // utility
        public Vector2 GetImageSize()
        {
            if (mTexture != null)
            {
                return new Vector2(mTexture.Width, mTexture.Height);
            }
            return Vector2.Zero;
        }

        public bool HasValidImage()
        {
            return mTexture != null &&
                   ((ImageType == ImageType.Static) ||
                    (ImageType == ImageType.SpriteSheet && spriteSheetConfig.TotalFrames > 0));
        }
    }
}