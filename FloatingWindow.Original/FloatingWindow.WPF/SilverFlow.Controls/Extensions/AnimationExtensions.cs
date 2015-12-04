using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace SilverFlow.Controls.Extensions
{
    /// <summary>
    /// Useful Silverlight animation extensions
    /// </summary>
    public static class AnimationExtensions
    {
        /// <summary>
        /// Animates specified property of the object.
        /// </summary>
        /// <param name="target">The target object to animate.</param>
        /// <param name="propertyPath">Property path, e.g. Canvas.Top.</param>
        /// <param name="from">Animation's starting value.</param>
        /// <param name="to">Animation's ending value.</param>
        /// <param name="milliseconds">Duration of the animation in milliseconds.</param>
        /// <param name="easingFunction">Easing function applied to the animation.</param>
        /// <param name="completed">Event handler called when animation completed.</param>
        /// <returns>Returns started storyboard.</returns>
        public static Storyboard AnimateDoubleProperty(this DependencyObject target, string propertyPath, double? from, double? to, double milliseconds,
            IEasingFunction easingFunction = null, EventHandler completed = null)
        {
            Duration duration = new Duration(TimeSpan.FromMilliseconds(milliseconds));
            DoubleAnimation doubleAnimation = new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = duration,
                EasingFunction = easingFunction
            };

            Storyboard storyboard = new Storyboard();
            storyboard.Duration = duration;
            storyboard.Children.Add(doubleAnimation);

            Storyboard.SetTarget(doubleAnimation, target);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(propertyPath));

            if (completed != null)
                storyboard.Completed += completed;

            storyboard.Begin();

            return storyboard;
        }

        /// <summary>
        /// Fade in the specified object.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="milliseconds">Duration of the animation in milliseconds.</param>
        /// <param name="completed">Event handler called when animation completed.</param>
        /// <returns>Returns started storyboard.</returns>
        public static Storyboard FadeIn(this DependencyObject target, double milliseconds, EventHandler completed = null)
        {
            return AnimateDoubleProperty(target, "Opacity", 0, 1, milliseconds, null, completed);
        }

        /// <summary>
        /// Fade out the specified object.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="milliseconds">Duration of the animation in milliseconds.</param>
        /// <param name="completed">Event handler called when animation completed.</param>
        /// <returns>Returns started storyboard.</returns>
        public static Storyboard FadeOut(this DependencyObject target, double milliseconds, EventHandler completed = null)
        {
            return AnimateDoubleProperty(target, "Opacity", 1, 0, milliseconds, null, completed);
        }

        /// <summary>
        /// Moves and resizes the object.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="storyboard">The storyboard.</param>
        /// <param name="position">Ending position.</param>
        /// <param name="width">New width.</param>
        /// <param name="height">New height.</param>
        /// <param name="milliseconds">Duration of the animation in milliseconds.</param>
        public static void MoveAndResize(this DependencyObject target, Storyboard storyboard, Point position, double width, double height,
            double milliseconds)
        {
            Duration duration = new Duration(TimeSpan.FromMilliseconds(milliseconds));
            DoubleAnimation doubleAnimation1 = new DoubleAnimation(width, duration);
            DoubleAnimation doubleAnimation2 = new DoubleAnimation();
            PointAnimation pointAnimation = new PointAnimation(position, duration);

            doubleAnimation2.Duration = duration;
            if (!height.IsNotSet())
                doubleAnimation2.To = height;

            storyboard.Stop();
            storyboard.Children.Clear();
            storyboard.Duration = duration;

            storyboard.Children.Add(doubleAnimation1);
            storyboard.Children.Add(doubleAnimation2);
            storyboard.Children.Add(pointAnimation);

            Storyboard.SetTarget(doubleAnimation1, target);
            Storyboard.SetTarget(doubleAnimation2, target);
            Storyboard.SetTarget(pointAnimation, target);

            Storyboard.SetTargetProperty(doubleAnimation1, new PropertyPath("(Width)"));
            Storyboard.SetTargetProperty(doubleAnimation2, new PropertyPath("(Height)"));
            Storyboard.SetTargetProperty(pointAnimation, new PropertyPath("(Position)"));

            storyboard.Begin();
        }

        /// <summary>
        /// Animates the translate transform object, responsible for displacement of the target.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="storyboard">The storyboard.</param>
        /// <param name="to">Animation's ending value.</param>
        /// <param name="seconds">Duration of the animation in seconds.</param>
        /// <param name="easingFunction">Easing function applied to the animation.</param>
        public static void AnimateTranslateTransform(this DependencyObject target, Storyboard storyboard, Point to,
            double seconds, IEasingFunction easingFunction = null)
        {
            Duration duration = new Duration(TimeSpan.FromSeconds(seconds));

            DoubleAnimation doubleAnimationX = new DoubleAnimation()
            {
                To = to.X,
                Duration = duration,
                EasingFunction = easingFunction
            };

            DoubleAnimation doubleAnimationY = new DoubleAnimation()
            {
                To = to.Y,
                Duration = duration,
                EasingFunction = easingFunction
            };

            storyboard.Stop();
            storyboard.Children.Clear();
            storyboard.Duration = duration;
            storyboard.Children.Add(doubleAnimationX);
            storyboard.Children.Add(doubleAnimationY);

            Storyboard.SetTarget(doubleAnimationX, target);
            Storyboard.SetTarget(doubleAnimationY, target);
            Storyboard.SetTargetProperty(doubleAnimationX, (target as UIElement).GetPropertyPathForTranslateTransformX());
            Storyboard.SetTargetProperty(doubleAnimationY, (target as UIElement).GetPropertyPathForTranslateTransformY());

            storyboard.Begin();
        }
    }
}
