using DotsAndBoxes.Core.Entities;
using DotsAndBoxes.Core.Interfaces;
using System.Drawing;
using System.Linq;

namespace DotsAndBoxes.Core.Game;

public class GameLogic : IGameLogic
{
    #region - needs -
    private List<Boxes> m_Boxes;
    #endregion

    #region - ctor -
    public GameLogic()
    {
    }

    #endregion

    #region - properties -

    #region - private properties -
    private int _NumberOfPlayer { get; set; }
    #endregion

    #region - public properties -

    #endregion

    #endregion

    #region - methods -

    #region - public methods -

    #region [GetGameboard]
    public List<Boxes> GetGameboard()
    {
        return m_Boxes;
    }
    #endregion

    #region [GetResultOfTurn]
    public List<Boxes> GetResultOfTurn(Point point1, Point point2, int player)
    {
        var result = new List<Boxes>();
        var affectedBoxes = m_Boxes.Where(x => x.Lines.Any(y => y.IsPointPairIsLine(point1, point2))).ToList();
        if (!affectedBoxes.Any() || affectedBoxes.Any(x => x.Player != -1))
            return result;
        affectedBoxes.ForEach(x => x.SetPlayerByLine(point1, point2, player));
        affectedBoxes.Where(x => x.IsCompleted()).ToList().ForEach(x => result.Add(x));

        return result;
    }
    #endregion

    public bool IsGameEnd()
    {
        return m_Boxes.All(b => b.IsCompleted());
    }

    #region [SetBoundariesAndPlayerNumbers]
    public void SetBoundariesAndPlayerNumbers(int length, int height, int numberOfPlayers)
    {
        if (m_Boxes != null && m_Boxes.Any())
            m_Boxes.Clear();
        if (m_Boxes == null)
            m_Boxes = new List<Boxes>();

        _NumberOfPlayer = numberOfPlayers;

        for (int i = 0; i < height - 1; i++)
        {
            for (int j = 0; j < length - 1; j++)
            {
                m_Boxes.Add(new Boxes(
                   upperLeft: new Point(i, j),
                   upperRight: new Point(i, j + 1),
                   lowerLeft: new Point(i + 1, j),
                   lowerRight: new Point(i + 1, j + 1)
                ));
            }
        }
    }
    #endregion

    #endregion

    #region - private methods -

    #endregion

    #region - override - 

    #endregion

    #endregion
}