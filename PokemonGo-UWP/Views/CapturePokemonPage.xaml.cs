using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using PokemonGo_UWP.Utils;
using Windows.UI.Xaml.Navigation;
using PokemonGo.RocketAPI;
using POGOProtos.Inventory.Item;
using Template10.Common;

namespace PokemonGo_UWP.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CapturePokemonPage : Page
    {
        public CapturePokemonPage()
        {
            InitializeComponent();
            // Setup catch stats translation
            Loaded += (s, e) =>
            {
                ShowCatchStatsModalAnimation.From = CatchStatsTranslateTransform.Y = ActualHeight;
                // HACK - somehow binding doesn't work as expected so we manually disable the item if count is 0
                LaunchPokeballButton.IsEnabled =
                    LaunchPokeballButton.IsHitTestVisible = ViewModel.SelectAvailablePokeBall().Count > 0;
                AudioUtils.PlaySound(AudioUtils.ENCOUNTER_POKEMON);
            };
        }

        #region Item Throw Update

        private async void PokeballUpdateLoop(ThreadPoolTimer timer)
        {
            if (UpdateLoopMutex.WaitOne(0))
            {
                var curTime = DateTime.Now;

                // timeDelta is the seconds since last update
                var timeDelta = (curTime - prevTime).Milliseconds/1000f;

                var gravity = new Vector3(0, 300f, 0);

                // Apply basic Kinematics
                ThrowItemPosition += ThrowItemVelocity*timeDelta + .5f*gravity*timeDelta*timeDelta;
                ThrowItemVelocity += gravity*timeDelta;

                /*
                Logger.Write("Position" + ThrowItemPosition.X + ", " + ThrowItemPosition.Y + ", " + ThrowItemPosition.Z);
                Logger.Write("Velocity" + ThrowItemVelocity.X + ", " + ThrowItemVelocity.Y + ", " + ThrowItemVelocity.Z);
                */

                prevTime = curTime;

                // Shotty attempt at converting from world space to screen space without a matrix
                var translateX = ThrowItemPosition.X*Math.Max(1.0f - ThrowItemPosition.Z/400.0f, 0.0f);
                var translateY = ThrowItemPosition.Y - ThrowItemPosition.Z;
                var scaleX = Math.Max(1.0f - ThrowItemPosition.Z/200.0f, 0.0f);
                var scaleY = scaleX;

                var pokeballStopped = false;
                var pokemonHit = false;

                if (Vector3.DistanceSquared(PokemonPosition, ThrowItemPosition) < PokemonRadiusSq)
                {
                    // We hit the pokemon!
                    pokeballStopped = true;
                    pokemonHit = true;
                    timer.Cancel();
                    Logger.Write("Hit Pokemon! " + ThrowItemPosition.X + ", " + ThrowItemPosition.Y + ", " +
                                 ThrowItemPosition.Z);
                }
                else if (ThrowItemPosition.Y > 50)
                {
                    // We missed the pokemon...
                    timer.Cancel();
                    pokeballStopped = true;
                    Logger.Write("Missed Pokemon! " + ThrowItemPosition.X + ", " + ThrowItemPosition.Y + ", " +
                                 ThrowItemPosition.Z);
                }

                UpdateLoopMutex.ReleaseMutex();

                await PokeballTransform.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    PokeballTransform.TranslateX = translateX;
                    PokeballTransform.TranslateY = translateY;
                    PokeballTransform.ScaleX = scaleX;
                    PokeballTransform.ScaleY = scaleY;
                    if (pokeballStopped)
                    {
                        if (pokemonHit)
                        {
                            // TODO: il casino è qua, se parte l'animazione poi non funziona più il movimento

                            ViewModel.UseSelectedCaptureItem.Execute(true);
                            var last = ViewModel.LastItemUsed;
                            if (last != null)
                            {
                                if (last == ItemId.ItemRazzBerry ||
                                    last == ItemId.ItemBlukBerry ||
                                    last == ItemId.ItemNanabBerry ||
                                    last == ItemId.ItemPinapBerry ||
                                    last == ItemId.ItemWeparBerry)
                                {
                                    ReInitItemLocation();
                                    return;
                                }
                            }
                            CatchStarted.Begin();
                            LaunchPokeballButton.IsEnabled = false;
                        }
                        else
                        {
                            // TODO: move the missed command if you want
                            ViewModel.UseSelectedCaptureItem.Execute(false);
                            ReInitItemLocation();
                        }
                    }
                });
            }
        }

        private void ReInitItemLocation()
        {
            PokeballTransform.TranslateX = InitItemX;
            PokeballTransform.TranslateY = InitItemY;
            PokeballTransform.ScaleX = 1;
            PokeballTransform.ScaleY = 1;
            LaunchPokeballButton.IsEnabled = true;
        }

        #endregion

        #region Item Throw Members

        /// <summary>
        ///     The X Position of the item before initiaiating a throw. We use this
        ///     track of where we started so that we can revert back to there.
        ///     Note: There is probably a better way of doing this...
        /// </summary>
        private float InitItemX;

        /// <summary>
        ///     The Y Position of the item before initiaiating a throw. We use this
        ///     track of where we started so that we can revert back to there.
        ///     Note: There is probably a better way of doing this...
        /// </summary>
        private float InitItemY;

        /// <summary>
        ///     List of the past positions while finger is down for the throw.
        ///     We will only consider the most recent 300ms of finger down.
        /// </summary>
        private readonly Queue<Tuple<Vector3, DateTime>> PastPositions = new Queue<Tuple<Vector3, DateTime>>();

        /// <summary>
        ///     The position of the pokemon in world space
        ///     TODO: Load this from the pokemon and update with pokemon movement (ie. flying pokemon should move up and down)
        /// </summary>
        private readonly Vector3 PokemonPosition = new Vector3(0, -50, 100);

        /// <summary>
        ///     The radius of the pokemon in world space, squared to avoid a sqrt later
        ///     TODO: Load this from the pokemon and update with pokemon movement (ie. flying pokemon should move up and down)
        /// </summary>
        private readonly float PokemonRadiusSq = 60*60;

        /// <summary>
        ///     Current velocity of the item durring a throw in world space
        /// </summary>
        private Vector3 ThrowItemVelocity;

        /// <summary>
        ///     Current position of the item durring a throw in world space
        /// </summary>
        private Vector3 ThrowItemPosition;

        /// <summary>
        ///     Lock for the update loop
        /// </summary>
        private volatile Mutex UpdateLoopMutex = new Mutex();

        /// <summary>
        ///     Time that the previous update executed
        /// </summary>
        private DateTime prevTime;

        #endregion

        #region Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SubscribeToCaptureEvents();
        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            UnsubscribeToCaptureEvents();
        }

        #endregion

        #region Handlers

        private void SubscribeToCaptureEvents()
        {
            ViewModel.CatchSuccess += GameManagerViewModelOnCatchSuccess;
            ViewModel.CatchEscape += GameManagerViewModelOnCatchEscape;
            ViewModel.CatchFlee += GameManagerViewModelOnCatchFlee;
            // Add also handlers to enable the button once the animation is done
            CatchEscape.Completed += (s, e) =>
            {
                PokeballTransform.TranslateX = 0;
                PokeballTransform.TranslateY = 0;
                PokeballTransform.ScaleX = 1;
                PokeballTransform.ScaleY = 1;
                LaunchPokeballButton.RenderTransform = PokeballTransform;
                LaunchPokeballButton.IsEnabled = true;
            };
            CatchFlee.Completed += (s, e) =>
            {
                // Go back once the animation finishes
                BootStrapper.Current.NavigationService.GoBack();
            };
        }

        private void UnsubscribeToCaptureEvents()
        {
            ViewModel.CatchSuccess -= GameManagerViewModelOnCatchSuccess;
            ViewModel.CatchEscape -= GameManagerViewModelOnCatchEscape;
            ViewModel.CatchFlee -= GameManagerViewModelOnCatchFlee;
        }

        private void GameManagerViewModelOnCatchEscape(object sender, EventArgs eventArgs)
        {
            CatchStarted.Stop();
            CatchEscape.Begin();
        }

        private void GameManagerViewModelOnCatchSuccess(object sender, EventArgs eventArgs)
        {
            CatchStarted.Stop();
            ShowCatchStatsModalStoryboard.Begin();
        }

        private void GameManagerViewModelOnCatchFlee(object sender, EventArgs eventArgs)
        {
            CatchEscape.Completed += (s, e) =>
            {
                LaunchPokeballButton.IsEnabled = false;
                CatchFlee.Begin();
            };
            CatchStarted.Stop();
            CatchEscape.Begin();
        }


        private void LaunchPokeballButton_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            Logger.Write("Manipulation Started...");

            // Show the ball a little bigger when held
            PokeballTransform.ScaleX *= 1.05f;
            PokeballTransform.ScaleY *= 1.05f;

            // Store this so that we can revert back to this poition
            InitItemX = (float) PokeballTransform.TranslateX;
            InitItemY = (float) PokeballTransform.TranslateY;

            PastPositions.Clear();
            PastPositions.Enqueue(new Tuple<Vector3, DateTime>(
                new Vector3(InitItemX, InitItemY, 0),
                DateTime.Now
                ));
        }

        private void LaunchPokeballButton_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            PokeballTransform.TranslateX += e.Delta.Translation.X;
            PokeballTransform.TranslateY += e.Delta.Translation.Y;

            // Track where we are now to use in later calculation
            PastPositions.Enqueue(new Tuple<Vector3, DateTime>(
                new Vector3((float) PokeballTransform.TranslateX, (float) PokeballTransform.TranslateY, 0),
                DateTime.Now
                ));

            // Remove anything from the queue that is more than 300ms old, we don't
            // want to track that far in the past
            while (PastPositions.Count > 1 && (PastPositions.Peek().Item2 - DateTime.Now).Milliseconds > 300)
            {
                PastPositions.Dequeue();
            }
        }

        private void LaunchPokeballButton_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Logger.Write("Manipulation Completed...");

            // Disable the pokeball so that we can't try and throw it again
            LaunchPokeballButton.IsEnabled = false;

            var EndingX = (float) PokeballTransform.TranslateX;
            var EndingY = (float) PokeballTransform.TranslateY;

            // Pull out of the history where our finger was ~300ms ago
            var start = PastPositions.Peek();
            var startTime = start.Item2;
            var startPos = start.Item1;

            // TODO: Use PastPositions to determine if this is a curve ball, and if it is
            // apply a force in the direction of the curve

            // Get some details that we will need to do math with
            var displacement = new Vector2(EndingX - startPos.X, EndingY - startPos.Y);
            var distance = displacement.Length();
            var throwDirection = Vector3.Normalize(new Vector3(displacement.X, displacement.Y, 100.0f));
            var timeDelta = (DateTime.Now - startTime).Milliseconds;

            // Set our initial position and velocity in world space
            ThrowItemPosition = new Vector3(EndingX, EndingY, 0);
            ThrowItemVelocity = distance*100.0f/timeDelta*throwDirection;

            /*
            Logger.Write("Init throwDirection " + throwDirection.X + ", " + throwDirection.X + ", " + throwDirection.Z);
            Logger.Write("Init TimeDelta " + timeDelta);
            Logger.Write("Init Position " + ThrowItemPosition.X + ", " + ThrowItemPosition.Y + ", " + ThrowItemPosition.Z);
            Logger.Write("Init Velocity " + ThrowItemVelocity.X + ", " + ThrowItemVelocity.Y + ", " + ThrowItemVelocity.Z);
            */

            prevTime = DateTime.Now;
            ThreadPoolTimer.CreatePeriodicTimer(PokeballUpdateLoop, TimeSpan.FromMilliseconds(20));
        }

        #endregion
    }
}
