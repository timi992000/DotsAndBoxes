using DotsAndBoxes.Core.Converter;
using DotsAndBoxes.Core.Entities;
using DotsAndBoxes.Core.Enums;
using DotsAndBoxes.Core.Game;
using DotsAndBoxes.Core.Interfaces;
using DotsAndBoxes.WPF.HelperObjects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DotsAndBoxes.WPF
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private const int m_defaultFieldSize = 5;
        private const int m_defaultEllipseSize = 20;
        private const int m_defaultBoxSize = 100;
        private const int EdgeZIndex = 1;
        private const int BoxZIndex = 50;
        private const int PointZIndex = 100;
        private Canvas m_GameCanvas;
        private readonly Brush EllipseNormalColor = Brushes.Gray;
        private readonly Brush EllipseHoveredColor = Brushes.Yellow;
        private readonly Brush EdgeColor = Brushes.Blue;
        private Ellipse HoveredEllipse;
        private List<Edge> AllEdges;
        private ePlayer CurrentPlayer = ePlayer.Red;
        private Queue<ePlayer> PlayerTurnQueue;
        private Dictionary<ePlayer, int> PlayersBoxes;

        public IGameLogic GameBL;

        public event PropertyChangedEventHandler? PropertyChanged;
        public Brush CurrentPlayerBrush => __GetPlayerDependendBrush();
        public ObservableCollection<Ellipse> ClickedEllipses { get; set; }
        public List<System.Drawing.Point> ClickedPoints { get; set; }
        public Dictionary<System.Drawing.Point, Ellipse> AllPoints { get; set; }

        //The fieldsize f. e. 10 x 10
        public int FieldSize { get; set; }
        //The heigth and width of one box
        public int BoxSize { get; set; }
        //The size of the points to connect
        public int EllipseSize { get; set; }
        public bool IsRedPlayerChecked { get; set; }
        public bool IsYellowPlayerChecked { get; set; }
        public bool IsGreenPlayerChecked { get; set; }
        public bool IsBluePlayerChecked { get; set; }
        public double WindowHeight { get; set; }
        public double WindowWidth { get; set; }

        public MainWindowViewModel(Canvas GameCanvas)
        {
            GameBL = new GameLogic();
            m_GameCanvas = GameCanvas;
            ClickedEllipses = new ObservableCollection<Ellipse>();
            ClickedPoints = new List<System.Drawing.Point>();
            AllPoints = new Dictionary<System.Drawing.Point, Ellipse>();
            __AttachEvents();
            __DrawGameField();
        }


        public void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public void DrawGameField()
        {
            __ClearSelected();
            __DrawGameField(false);
        }

        private void __AttachEvents()
        {
            m_GameCanvas.MouseLeftButtonDown += __LeftMouseButtonDown;
            ClickedEllipses.CollectionChanged += __ClickedEllipsesChanged;
        }

        private void __ClickedEllipsesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (ClickedEllipses.Count == 2)
            {
                __DrawEdgesOnClickedPoints();
                ClickedEllipses.ToList().ForEach(e => e.Fill = EllipseNormalColor);
                __ClearSelected();
            }
        }

        private void __ClearSelected()
        {
            ClickedEllipses.Clear();
            ClickedPoints.Clear();
        }

        private Brush __GetPlayerDependendBrush()
        {
            switch (CurrentPlayer)
            {
                case ePlayer.Red:
                    return Brushes.Red;
                case ePlayer.Yellow:
                    return Brushes.Yellow;
                case ePlayer.Green:
                    return Brushes.Green;
                case ePlayer.Blue:
                    return Brushes.Blue;
                default:
                    return Brushes.Red;
            }
        }

        private void __DrawEdgesOnClickedPoints()
        {
            if (ClickedEllipses.Count != 2)
                return;
            var NewEdge = new Edge { CanvasFrom = ClickedPoints.First(), CanvasTo = ClickedPoints.Last(), DrawedFrom = ePlayer.Red };
            if (AllEdges.Any(a => ClickedPoints.Contains(a.CanvasFrom) && ClickedPoints.Contains(a.CanvasTo)))
            {
                //Edge is already set
                return;
            }

            var MinX = ClickedPoints.Min(x => x.X);
            var MaxX = ClickedPoints.Max(x => x.X + EllipseSize);
            var MinY = ClickedPoints.Min(x => x.Y);
            var MaxY = ClickedPoints.Max(x => x.Y + EllipseSize);
            var EdgeHeight = MaxY - MinY;
            var EdgeWidth = MaxX - MinX;
            var Direction = __GetDirectionFromClickedPoints();
            if (Direction == eDirection.Vertical)
            {
                EdgeHeight -= EllipseSize * 2;
            }
            else
            {
                EdgeWidth -= EllipseSize * 2;
            }
            var Edge = new Rectangle()
            {
                Fill = __GetPlayerDependendBrush() /*EdgeColor*/,
                Height = EdgeHeight,
                Width = EdgeWidth,
            };
            NewEdge.EdgeInstance = Edge;
            m_GameCanvas.Children.Add(Edge);
            if (Direction == eDirection.Vertical)
            {
                Canvas.SetLeft(Edge, MinX);
                Canvas.SetTop(Edge, MinY + EllipseSize);
            }
            else
            {
                Canvas.SetLeft(Edge, MinX + EllipseSize);
                Canvas.SetTop(Edge, MinY);
            }
            Panel.SetZIndex(Edge, EdgeZIndex);
            AllEdges.Add(NewEdge);

            NewEdge.SimulatedFrom = AllPoints.First(x => x.Value == ClickedEllipses.First()).Key;
            NewEdge.SimulatedTo = AllPoints.First(x => x.Value == ClickedEllipses.Last()).Key;

            var TurnResult = GameBL.GetResultOfTurn(NewEdge.SimulatedFrom, NewEdge.SimulatedTo, (int)CurrentPlayer);
            if (TurnResult.Any())
            {
                __DrawBoxOnTurnResult(TurnResult);
                __CheckAndHandleEndGame();
            }
            else
                __NextPlayer();
        }

        private void __DrawBoxOnTurnResult(List<Boxes> turnResultBoxes)
        {
            foreach (var box in turnResultBoxes)
            {
                int MinX = int.MaxValue;
                int MinY = int.MaxValue;
                var FoundEdges = new List<Rectangle>();
                foreach (var line in box.Lines)
                {
                    if (line.Point1.X < MinX)
                        MinX = line.Point1.X;
                    if (line.Point2.X < MinX)
                        MinX = line.Point2.X;
                    if (line.Point1.Y < MinY)
                        MinY = line.Point1.Y;
                    if (line.Point2.Y < MinY)
                        MinY = line.Point2.Y;
                    var TmpEdge = AllEdges.FirstOrDefault(ae => (line.Point1 == ae.SimulatedFrom || line.Point1 == ae.SimulatedTo) && (line.Point2 == ae.SimulatedFrom || line.Point2 == ae.SimulatedTo));
                    if (TmpEdge != null)
                        FoundEdges.Add(TmpEdge.EdgeInstance);
                }
                var DrawBox = FoundEdges.Count == 4;
                if (DrawBox)
                {
                    var HalfEllipseSize = EllipseSize / 2;
                    var Edge = new Rectangle()
                    {
                        Fill = __GetPlayerDependendBrush(),
                        Height = BoxSize,
                        Width = BoxSize,
                    };
                    FoundEdges.ForEach(e => e.Fill = Brushes.Transparent);
                    m_GameCanvas.Children.Add(Edge);
                    Canvas.SetLeft(Edge, (MinX * BoxSize) + HalfEllipseSize);
                    Canvas.SetTop(Edge, (MinY * BoxSize) + HalfEllipseSize);
                    Panel.SetZIndex(Edge, BoxZIndex);
                    if (!PlayersBoxes.ContainsKey(CurrentPlayer))
                        PlayersBoxes.Add(CurrentPlayer, 0);
                    PlayersBoxes[CurrentPlayer]++;
                }
            }
        }

        private eDirection __GetDirectionFromClickedPoints()
        {
            if (ClickedPoints.Count != 2)
                throw new System.Exception("Clicked Points count should be 2 if you check direction");
            var FirstPoint = ClickedPoints.First();
            var SecondPoint = ClickedPoints.Last();
            if (FirstPoint.X == SecondPoint.X && FirstPoint.Y != SecondPoint.Y)
                return eDirection.Vertical;
            else
                return eDirection.Horizontal;
        }

        private void __CheckAndHandleEndGame()
        {
            if (GameBL.IsGameEnd())
            {
                AllPoints.ToList().ForEach(x => x.Value.IsEnabled = false);
                var PlayerWhoWon = PlayersBoxes.OrderByDescending(x => x.Value).First();
                MessageBox.Show($"Game End, Player {PlayerWhoWon.Key} Won with {PlayerWhoWon.Value} Boxes", "Game is over", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void __LeftMouseButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (HoveredEllipse != null)
            {
                if (!ClickedEllipses.Contains(HoveredEllipse))
                {
                    var NewClickedPoint = new System.Drawing.Point(Canvas.GetLeft(HoveredEllipse).ToInt32Value(), Canvas.GetTop(HoveredEllipse).ToInt32Value());
                    if (!__CheckCanClickOnEllipse(NewClickedPoint))
                        return;
                    ClickedPoints.Add(NewClickedPoint);
                    ClickedEllipses.Add(HoveredEllipse);
                }
                else
                {
                    ClickedPoints.Remove(new System.Drawing.Point(Canvas.GetLeft(HoveredEllipse).ToInt32Value(), Canvas.GetTop(HoveredEllipse).ToInt32Value()));
                    ClickedEllipses.Remove(HoveredEllipse);
                }
            }
        }

        private bool __CheckCanClickOnEllipse(System.Drawing.Point NewClickedPoint)
        {
            if (ClickedEllipses.Count == 1)
            {
                var FirstClickedPoint = ClickedPoints.First();
                //Point Top
                if (NewClickedPoint.X == FirstClickedPoint.X
                    && NewClickedPoint.Y == FirstClickedPoint.Y - BoxSize)
                    return true;
                //Point Bottom
                else if (NewClickedPoint.X == FirstClickedPoint.X
                   && NewClickedPoint.Y == FirstClickedPoint.Y + BoxSize)
                    return true;
                //Point Left
                else if (NewClickedPoint.X == FirstClickedPoint.X - BoxSize
                   && NewClickedPoint.Y == FirstClickedPoint.Y)
                    return true;
                //Point Right
                else if (NewClickedPoint.X == FirstClickedPoint.X + BoxSize
                   && NewClickedPoint.Y == FirstClickedPoint.Y)
                    return true;
                return false;
            }
            return true;
        }

        private void __DrawGameField(bool defaultValues = true)
        {
            if (defaultValues)
                __SetDefaultValues();
            __EnqueuePlayerTurns();
            GameBL.SetBoundariesAndPlayerNumbers(FieldSize, FieldSize, PlayerTurnQueue.Count);
            var Boxes = GameBL.GetGameboard();
            AllEdges = new List<Edge>();
            PlayersBoxes = new Dictionary<ePlayer, int>();
            __RecalculateGameField();
            __BuildCanvasPoints(Boxes);
        }

        private void __EnqueuePlayerTurns()
        {
            PlayerTurnQueue = new Queue<ePlayer>();
            if (IsRedPlayerChecked)
                PlayerTurnQueue.Enqueue(ePlayer.Red);
            if (IsYellowPlayerChecked)
                PlayerTurnQueue.Enqueue(ePlayer.Yellow);
            if (IsGreenPlayerChecked)
                PlayerTurnQueue.Enqueue(ePlayer.Green);
            if (IsBluePlayerChecked)
                PlayerTurnQueue.Enqueue(ePlayer.Blue);
            __NextPlayer();
        }

        private void __NextPlayer()
        {
            CurrentPlayer = PlayerTurnQueue.Dequeue();
            PlayerTurnQueue.Enqueue(CurrentPlayer);
            OnPropertyChanged(nameof(CurrentPlayerBrush));
        }

        private void __SetDefaultValues()
        {
            IsRedPlayerChecked = true;
            IsYellowPlayerChecked = true;
            FieldSize = m_defaultFieldSize;
            BoxSize = m_defaultBoxSize;
            EllipseSize = m_defaultEllipseSize;
        }

        private void __RecalculateGameField()
        {
            var Size = FieldSize * BoxSize;
            m_GameCanvas.Width = Size;
            m_GameCanvas.Height = Size;
            WindowWidth = Size + Size * 0.15;
            WindowHeight = Size + Size * 0.35;
            OnPropertyChanged(nameof(WindowWidth));
            OnPropertyChanged(nameof(WindowHeight));
        }

        private void __BuildCanvasPoints(List<Boxes> boxes)
        {
            if (m_GameCanvas == null)
                m_GameCanvas = new Canvas();
            m_GameCanvas.Children.Clear();
            AllPoints.Clear();
            double HalfPointSize = EllipseSize / 2;

            foreach (var box in boxes)
            {
                foreach (var boxLine in box.Lines)
                {
                    if (!AllPoints.ContainsKey(boxLine.Point1))
                        __DrawPointOnCanvas(boxLine.Point1);

                    if (!AllPoints.ContainsKey(boxLine.Point2))
                        __DrawPointOnCanvas(boxLine.Point2);

                }
            }
        }

        private void __DrawPointOnCanvas(System.Drawing.Point PointToDraw)
        {
            var Point = new Ellipse()
            {
                Fill = EllipseNormalColor,
                Width = EllipseSize,
                Height = EllipseSize,
            };
            __AttachPointEvents(Point);
            m_GameCanvas.Children.Add(Point);
            var NewX = PointToDraw.X * BoxSize;
            var NewY = PointToDraw.Y * BoxSize;
            Canvas.SetLeft(Point, NewX);
            Canvas.SetTop(Point, NewY);
            AllPoints.Add(new System.Drawing.Point(PointToDraw.X, PointToDraw.Y), Point);
            Panel.SetZIndex(Point, PointZIndex);
        }

        private void __AttachPointEvents(Ellipse point)
        {
            point.MouseEnter += (object sender, MouseEventArgs e) =>
            {
                HoveredEllipse = point;
                point.Fill = EllipseHoveredColor;
            };
            point.MouseLeave += (object sender, MouseEventArgs e) =>
            {
                if (!ClickedEllipses.Contains(HoveredEllipse))
                    point.Fill = EllipseNormalColor;
                HoveredEllipse = null;
            };
        }
    }
}