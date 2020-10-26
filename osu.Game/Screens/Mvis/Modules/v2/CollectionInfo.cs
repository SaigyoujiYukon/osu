using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Mvis.Modules.v2
{
    public class CollectionInfo : Container
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        private Box flashBox;
        private OsuSpriteText collectionName;
        private OsuSpriteText collectionBeatmapCount;
        private Bindable<BeatmapCollection> collection = new Bindable<BeatmapCollection>();
        private List<BeatmapSetInfo> beatmapSets = new List<BeatmapSetInfo>();
        private BeatmapCover cover;

        [Cached]
        public readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue1);
        private BeatmapList beatmapList;
        private BindableBool isCurrentCollection = new BindableBool();

        public CollectionInfo()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colourProvider.Background3,
                    RelativeSizeAxes = Axes.Both
                },
                cover = new BeatmapCover(null)
                {
                    BackgroundBox = false,
                    UseBufferedBackground = true
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3.Opacity(0.5f)
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new Dimension[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.Distributed),
                    },
                    Content = new []
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                Name = "标题容器",
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                AutoSizeDuration = 300,
                                AutoSizeEasing = Easing.OutQuint,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colourProvider.Background3.Opacity(0.5f)
                                    },
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(12),
                                        Padding = new MarginPadding{ Horizontal = 35, Vertical = 25 },
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Children = new Drawable[]
                                        {
                                            collectionName = new OsuSpriteText
                                            {
                                                Font = OsuFont.GetFont(size: 50),
                                                RelativeSizeAxes = Axes.X,
                                                Text = "未选择收藏夹"
                                            },
                                            collectionBeatmapCount = new OsuSpriteText
                                            {
                                                Font = OsuFont.GetFont(size: 38),
                                                RelativeSizeAxes = Axes.X,
                                                Text = "请先选择一个收藏夹!"
                                            }
                                        }
                                    },
                                    flashBox = new Box
                                    {
                                        Height = 3,
                                        RelativeSizeAxes = Axes.X,
                                        Colour = Colour4.Gold,
                                        Anchor = Anchor.BottomLeft,
                                    }
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    listContainer = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding{Left = 35, Vertical = 20},
                                    },
                                    loadingSpinner = new LoadingSpinner(true)
                                    {
                                        Size = new Vector2(50)
                                    }
                                }
                            },
                        }
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            collection.BindValueChanged(OnCollectionChanged);
        }

        private void OnCollectionChanged(ValueChangedEvent<BeatmapCollection> v)
        {
            var c = v.NewValue;

            if (c == null)
            {
                ClearInfo();
                return;
            }

            beatmapSets.Clear();
            //From CollectionHelper.cs
            foreach (var item in c.Beatmaps)
            {
                //获取当前BeatmapSet
                var currentSet = item.BeatmapSet;

                //进行比对，如果beatmapList中不存在，则添加。
                if (!beatmapSets.Contains(currentSet))
                    beatmapSets.Add(currentSet);
            }

            collectionName.Text = c.Name.Value;
            collectionBeatmapCount.Text = $"{beatmapSets.Count}首歌曲";

            cover.updateBackground(beatmaps.GetWorkingBeatmap(beatmapSets.ElementAt(0).Beatmaps.First()));
            flashBox.FlashColour(Colour4.White, 1000, Easing.OutQuint);

            RefreshBeatmapSetList();
        }

        private CancellationTokenSource refreshTaskCancellationToken;
        private Container listContainer;
        private LoadingSpinner loadingSpinner;

        private void RefreshBeatmapSetList()
        {
            Task refreshTask;
            refreshTaskCancellationToken?.Cancel();
            refreshTaskCancellationToken = new CancellationTokenSource();

            beatmapList?.FadeOut(250).Then().Expire();
            loadingSpinner.Show();

            Task.Run(async () =>
            {
                refreshTask = Task.Run(() =>
                {
                    LoadComponentAsync(new BeatmapList(beatmapSets), newList =>
                    {
                        newList.IsCurrent.BindTo(isCurrentCollection);
                        beatmapList = newList;

                        listContainer.Add(newList);
                        newList.Show();
                        loadingSpinner.Hide();
                    }, refreshTaskCancellationToken.Token);
                }, refreshTaskCancellationToken.Token);

                await refreshTask;
            });
        }

        public void UpdateCollection(BeatmapCollection collection, bool isCurrent)
        {
            if (!isCurrent) flashBox.FadeColour(Colour4.Gold, 300, Easing.OutQuint);
            else flashBox.FadeColour(Color4Extensions.FromHex("#88b300"), 300, Easing.OutQuint);

            if ( collection != this.collection.Value && beatmapList != null )
            {
                beatmapList.IsCurrent.UnbindAll();
                beatmapList.IsCurrent.Value = false;
            }

            //设置当前选择是否为正在播放的收藏夹
            isCurrentCollection.Value = isCurrent;

            //将当前收藏夹设为collection
            this.collection.Value = collection;
        }

        private void ClearInfo()
        {
            cover.updateBackground(null);

            beatmapSets.Clear();
            beatmapList.ClearList();
            collectionName.Text = "未选择收藏夹";
            collectionBeatmapCount.Text = "请先选择一个收藏夹!";

            flashBox.FadeColour(Colour4.Gold);
        }
    }
}