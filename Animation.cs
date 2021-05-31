﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Glyph.Utilities
{
    public class Animation
    {
        List<AnimationFrame> frames = new List<AnimationFrame>();
        TimeSpan timeIntoAnimation;

        TimeSpan Duration
        {
            get
            {
                double totalSeconds = 0;
                foreach (var frame in frames)
                {
                    totalSeconds += frame.Duration.TotalSeconds;
                }

                return TimeSpan.FromSeconds(totalSeconds);
            }
        }

        public void AddFrame(Rectangle rectangle, TimeSpan duration)
        {
            AnimationFrame newFrame = new AnimationFrame()
            {
                SourceRectangle = rectangle,
                Duration = duration
            };

            frames.Add(newFrame);
        }

        public void Update(GameTime gameTime)
        {
            double secondsIntoAnimation = timeIntoAnimation.TotalSeconds + gameTime.ElapsedGameTime.TotalSeconds;
            double remainder = secondsIntoAnimation % Duration.TotalSeconds;
            timeIntoAnimation = TimeSpan.FromSeconds(remainder);
        }



        public Rectangle CurrentRectangle
        {
            get
            {
                AnimationFrame currentFrame = null;


                TimeSpan accumulatedTime = new TimeSpan();
                foreach (var frame in frames)
                {
                    if (accumulatedTime + frame.Duration >= timeIntoAnimation)
                    {
                        currentFrame = frame;
                        break;
                    }
                    else
                    {
                        accumulatedTime += frame.Duration;
                    }
                }

                if (currentFrame == null)
                {
                    return DefaultRectangle;
                }

                else
                {
                    return currentFrame.SourceRectangle;
                }
            }
        }

        public Rectangle DefaultRectangle
        {
            get
            {
                if (frames.Count > 1)
                {
                    return frames[1].SourceRectangle;
                }
                else
                {
                    return frames[0].SourceRectangle;
                }
            }
        } 
    }
}