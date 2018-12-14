﻿using System;
using System.ComponentModel;
using Airbnb.Lottie;
using Foundation;
using Lottie.Forms;
using Lottie.Forms.EventArguments;
using Lottie.Forms.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(AnimationView), typeof(AnimationViewRenderer))]

namespace Lottie.Forms.iOS.Renderers
{
    public class AnimationViewRenderer : ViewRenderer<AnimationView, LOTAnimationView>
    {
        private LOTAnimationView _animationView;
        private UITapGestureRecognizer _gestureRecognizer;

        /// <summary>
        ///   Used for registration with dependency service
        /// </summary>
        public new static void Init()
        {
            // needed because of this linker issue: https://bugzilla.xamarin.com/show_bug.cgi?id=31076
#pragma warning disable 0219
            var dummy = new AnimationViewRenderer();
#pragma warning restore 0219
        }

        protected override void OnElementChanged(ElementChangedEventArgs<AnimationView> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                e.OldElement.OnPlay -= OnPlay;
                e.OldElement.OnPause -= OnPause;
                e.OldElement.OnPlayProgressSegment -= OnPlayProgressSegment;
                e.OldElement.OnPlayFrameSegment -= OnPlayFrameSegment;
            }

            if (e.NewElement == null) return;

            e.NewElement.OnPlay += OnPlay;
            e.NewElement.OnPause += OnPause;
            e.NewElement.OnPlayProgressSegment += OnPlayProgressSegment;
            e.NewElement.OnPlayFrameSegment += OnPlayFrameSegment;

            InitAnimationViewForElement(e.NewElement);
        }

        private void OnPlay(object sender, EventArgs e)
        {
            _animationView?.PlayWithCompletion(PlaybackFinishedIfActually);
            Element.IsPlaying = true;
        }

        private void OnPlayProgressSegment(object sender, ProgressSegmentEventArgs e)
        {
            _animationView?.PlayFromProgress(e.From, e.To, PlaybackFinishedIfActually);
            Element.IsPlaying = true;
        }

        private void OnPlayFrameSegment(object sender, FrameSegmentEventArgs e)
        {
            _animationView?.PlayFromFrame(e.From, e.To, PlaybackFinishedIfActually);
            Element.IsPlaying = true;
        }

        private void OnPause(object sender, EventArgs e)
        {
            _animationView?.Pause();
            Element.IsPlaying = false;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Element == null)
                return;

            if (e.PropertyName == AnimationView.AnimationJsonProperty.PropertyName)
            {
                _animationView?.RemoveFromSuperview();
                _animationView?.RemoveGestureRecognizer(_gestureRecognizer);
                InitAnimationViewForElement(Element);
            }
            if (e.PropertyName == AnimationView.AnimationProperty.PropertyName)
            {
                _animationView?.RemoveFromSuperview();
                _animationView?.RemoveGestureRecognizer(_gestureRecognizer);
                InitAnimationViewForElement(Element);
            }

            if (_animationView == null)
                return;

            if (e.PropertyName == AnimationView.ProgressProperty.PropertyName)
            {
                _animationView.AnimationProgress = Element.Progress;
            }

            if (e.PropertyName == AnimationView.LoopProperty.PropertyName)
            {
                _animationView.LoopAnimation = Element.Loop;
            }

            if (e.PropertyName == AnimationView.SpeedProperty.PropertyName)
            {
                _animationView.AnimationSpeed = Element.Speed;
            }
        }

        private void InitAnimationViewForElement(AnimationView theElement)
        {
            if (!string.IsNullOrEmpty(theElement.AnimationJson))
            {
                var jsonData = NSData.FromString(theElement.AnimationJson);
                var jsonDictionary = (NSDictionary)NSJsonSerialization.Deserialize(jsonData, 0, out var error);
                _animationView = LOTAnimationView.AnimationFromJSON(jsonDictionary);
            }
            else if (!string.IsNullOrEmpty(theElement.Animation))
            {
                _animationView = new LOTAnimationView(NSUrl.FromFilename(theElement.Animation));
            }
            else
            {
                _animationView = new LOTAnimationView();
            }
            _animationView.AutoresizingMask = UIViewAutoresizing.All;
            _animationView.ContentMode = UIViewContentMode.ScaleAspectFit;
            _animationView.LoopAnimation = theElement.Loop;
            _animationView.AnimationSpeed = theElement.Speed;

            _gestureRecognizer = new UITapGestureRecognizer(theElement.Click);
            _animationView.AddGestureRecognizer(_gestureRecognizer);

            Element.Duration = TimeSpan.FromMilliseconds(_animationView.AnimationDuration);

            if (theElement.AutoPlay)
            {
                theElement.IsPlaying = true;
                _animationView.PlayWithCompletion(PlaybackFinishedIfActually);
            }
            else
            {
                theElement.IsPlaying = false;
            }

            if (_animationView != null)
            {
                SetNativeControl(_animationView);
                SetNeedsLayout();
            }
        }

        void PlaybackFinishedIfActually(bool animationFinished)
        {
            if (animationFinished && Element != null)
            {
                Element.IsPlaying = false;
                Element.PlaybackFinished();
            }
        }
    }
}
