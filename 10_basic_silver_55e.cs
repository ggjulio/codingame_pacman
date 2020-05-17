using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

class Player
{
    static void Main(string[] args)
    {
		Game game = new Game();

        while (true)
        {
			game.Sync();
			game.Play();
        }
    }
}

public abstract class Entity
{
	public Vector2 Position{get; set;}

	public Entity(Vector2 position)
	{
		this.Position = position;
	}

	public void Update(Vector2 position)
	{
		this.Position = position;
	}
	// public float Distance(Game game, Entity e)
	// {
	// 	return(Vector2.Distance(Position, e.Position));
	// }
	// public float Distance(Game game, Vector2 v)
	// {
	// 	return(Vector2.Distance(Position, v));
	// }

	public float Distance(Game game, Entity e)
	{
		return (float)game.BfsDepth(this.Position, e.Position);
	}
	public float Distance(Game game, Vector2 v)
	{
		return (float)game.BfsDepth(this.Position, v);
	}
	public float DistancePacsAsWall(Game game, Entity e)
	{
		return (float)game.BfsDepthPacsAsWall(this.Position, e.Position);
	}
	public float DistancePacsAsWall(Game game, Vector2 v)
	{
		return (float)game.BfsDepthPacsAsWall(this.Position, v);
	}
	
	public override string ToString()
	{
		return ($"Entity(Position:{Position})");
	}
}
public enum eSwitch{
	DEAD,
	ROCK,
	SCISSORS,
	PAPER,
}
public enum eAction{
	None,
	Move,
	Speed,
	Switch
}
public class Action
{
	public eAction Type{get;set;}
	public bool HasAction{get;set;}
	public bool IsMove{get;set;}
	public Vector2	TargetPosition{get;set;}
	public Entity  TargetEntity{get;set;}
	public eSwitch TargetSwitch{get;set;}

	public Action()
	{
		this.Type = eAction.None;
		this.HasAction = false;
		this.IsMove = false;
	}
	public Action(eAction type)
	{
		this.Type = type;
		this.HasAction =  type != eAction.None ? true : false;
		this.IsMove = type == eAction.Move ? true : false;
	}
	public Action(Vector2 targetPosition)
	{
		this.Type = eAction.Move;
		this.TargetPosition = targetPosition;
		this.HasAction = true;
		this.IsMove = true;
	}
	public Action(Entity targetEntity)
	{
		if (targetEntity == null)
		{
			this.Type = eAction.None;
			this.HasAction = false;
			return;
		}
		this.Type = eAction.Move;
		this.TargetPosition = targetEntity.Position;
		this.TargetEntity = targetEntity;
		this.HasAction = true;
		this.IsMove = true;
	}
	public Action(eSwitch targetSwitch)
	{
		this.Type = eAction.Switch;
		this.TargetSwitch = targetSwitch;
		this.HasAction = true;
		this.IsMove = false;
	}
}

public class Pac : Entity
{
	public int Id{get;}
	public bool Mine{get;}
	public List<(ushort Turn, Vector2 Position)> Positions{get;set;}
	public string TypeId{get;set;}
	public eSwitch Type{get;set;}
	public int SpeedTurnsLeft{get;set;}
	public int SpeedStep{get;set;}
	public int AbilityCooldown{get;set;}
	public bool	IsAlive{get;set;}
	public bool	IsVisible{get;set;}
	public string Label{get;set;}
	public Action Action{get;set;}
	public Action PreviousAction{get;set;}

	public Pac(int id, bool mine, ushort turn, Vector2 position, string typeId,
		int speedTurnsLeft, int abilityCooldown) : base(position)
	{
		this.Id = id;
		this.Mine = mine;
		this.Positions = new List<(ushort, Vector2)>();
		this.Positions.Insert(0, (turn, position));
		this.TypeId = typeId;
		this.Type = (eSwitch)Enum.Parse(typeof(eSwitch), typeId);
		this.SpeedTurnsLeft = speedTurnsLeft;
		this.SpeedStep = SpeedTurnsLeft > 0 ? 2 : 1;
		this.AbilityCooldown = abilityCooldown;
		this.IsAlive = this.Type != eSwitch.DEAD ? true : false;
		this.Action = new Action();
		this.IsVisible = true;
	}

	public void Update(ushort turn, Vector2 position, string typeId, int speedTurnsLeft, int abilityCooldown)
	{
		base.Update(position);
		this.Positions.Insert(0, (turn, position));
		this.TypeId = typeId;
		this.Type = (eSwitch)Enum.Parse(typeof(eSwitch), typeId);
		this.SpeedTurnsLeft = speedTurnsLeft;
		this.SpeedStep = SpeedTurnsLeft > 0 ? 2 : 1;
		this.AbilityCooldown = abilityCooldown;
		this.IsAlive = this.Type != eSwitch.DEAD ? true : false;
		this.Label = "";
		this.IsVisible = true;
	}
	public void Move(Vector2 targetPosition)
	{
		this.Action = new Action(targetPosition);
	}
	public void Move(Entity targetEntity)
	{
		this.Action = new Action(targetEntity);
	}
	public void Switch(eSwitch targetType)
	{
		this.Action = new Action(targetType);
	}
	public static eSwitch GetWeakType(eSwitch s)
	{
		switch (s)
		{
			case eSwitch.PAPER:
				return eSwitch.ROCK;
			case eSwitch.ROCK:
				return eSwitch.SCISSORS;
			case eSwitch.SCISSORS:
				return eSwitch.PAPER;
			default:
				return eSwitch.DEAD;
		}
	}
	public static eSwitch GetStrongType(eSwitch s)
	{
		switch (s)
		{
			case eSwitch.PAPER:
				return eSwitch.SCISSORS;
			case eSwitch.ROCK:
				return eSwitch.PAPER;
			case eSwitch.SCISSORS:
				return eSwitch.ROCK;
			default:
				return eSwitch.DEAD;
		}
	}
	public bool CanEat(Pac p)
	{
		if (this.Type == Pac.GetStrongType(p.Type))
			return true;
		return false;
	}
	public bool CanBeEatenBy(Pac p)
	{
		if (this.Type == Pac.GetWeakType(p.Type))
			return true;
		return false;
	}
	public void SwitchToEat(Pac p)
	{
		this.Action = new Action(Pac.GetStrongType(p.Type));
	}
	public void Speed()
	{
		this.Action = new Action(eAction.Speed);
	}
	public void ExecuteAction()
	{
		if (!this.IsAlive)
			return;
		switch (this.Action.Type)
		{
			case eAction.Move:
				Console.Write($"MOVE {this.Id} {this.Action.TargetPosition.X} {this.Action.TargetPosition.Y} {this.Label}|");
				break;
			case eAction.Switch:
				Console.Write($"SWITCH {this.Id} {this.Action.TargetSwitch.ToString()} {this.Label}|");
				break;
			case eAction.Speed:
				Console.Write($"SPEED {this.Id} {this.Label}|");
				break;
			default:
				Console.Write($"MOVE {this.Id} {this.Position.X} {this.Position.Y} BUG !|");
				break;
		}
		this.PreviousAction = this.Action;
	}
	public override string ToString()
	{
		return $"Pac(Id:{Id}; Mine:{Mine}; Position:{Position};\n    SpeedStep:{SpeedStep}; SpeedTurnsLeft:{SpeedTurnsLeft}; AbilityCooldown:{AbilityCooldown})";
	}
}

public class Pellet : Entity
{
	public int Value {get;}

	public Pellet(Vector2 position, int value) : base(position)
	{
		this.Value = value;
	}
}

public class Cell
{
	public bool   	IsWalkable{get;}
	public bool   	IsVisiblePellet{get;set;}
	public bool	  	HasPellet{get;set;}
	public bool	  	HasMyPac{get;set;}
	public bool	  	HasOpponentPac{get;set;}
	public Entity 	Inside{get; set;}	
	public Vector2	Position{get;}
	public Cell(char cell, Vector2 position)
	{
		this.IsWalkable = cell == ' ' ? true : false;
		if (IsWalkable)
		{
			this.HasPellet = true;
			Inside = new Pellet(position, 1);
		}
		this.IsVisiblePellet = false;
		this.Position = position;
	}
	public override string ToString()
	{
		string res;
		res  = $"Pos:{Position.ToString()} IsWalkable:{IsWalkable}";
		res += $" HasPellet:{HasPellet} IsVisiblePellet:{IsVisiblePellet}";
		res +=  " Inside:";
		if (Inside is Pac pa)
			res += "Pacman" + (pa.Mine ? "_Mine" : "_NOT_Mine");
		else if (Inside is Pellet pe)
			res += $"Pellet({pe.Position.ToString()})";
		else if (Inside == null)
			res += "NULL";
		else
			res += "Weird";
		return res;
	}
}
public class Grid
{
	public int Height{get; set;}
	public int Width{get; set;}
	public Cell[,] Map{get; set;}

	public Grid(int width, int height)
	{
		this.Width = width;
		this.Height = height;

		this.Map = new Cell[width, Height];
		
        for (int i = 0; i < Height; i++)
        {
			// one line of the grid: space " " is floor, pound "#" is wall
            string row = Console.ReadLine();
		    for (int j = 0; j < Width; j++)
				Map[j,i] = new Cell(row[j], new Vector2(j, i));
        }
	}
	public void Update(List<Pac> pacs, List<Pellet> pellets)
	{
		foreach (Cell c in Map)
		{
			if (c.Inside is Pac)
				c.Inside = null;
			c.HasMyPac = false;
			c.HasOpponentPac = false;
			c.IsVisiblePellet = false;
		}
		// To set Has pellet to false when the grid is discovered.
		foreach (Pac p in pacs.Where(e => e.Mine).ToList())
		{
			for (int i = 1; p.Position.X + i < this.Width && Map[(int)p.Position.X + i, (int)p.Position.Y].IsWalkable; i++)
			{
				Map[(int)p.Position.X + i, (int)p.Position.Y].HasPellet = false;
				Map[(int)p.Position.X + i, (int)p.Position.Y].Inside = null;
			}
			for (int i = -1; p.Position.X + i >= 0 && Map[(int)p.Position.X + i, (int)p.Position.Y].IsWalkable; i--)
			{
				Map[(int)p.Position.X + i, (int)p.Position.Y].HasPellet = false;
				Map[(int)p.Position.X + i, (int)p.Position.Y].Inside = null;
			}
			for (int i = 1; p.Position.Y + i < this.Height && Map[(int)p.Position.X, (int)p.Position.Y + i].IsWalkable; i++)
			{
				Map[(int)p.Position.X, (int)p.Position.Y + i].HasPellet = false;
				Map[(int)p.Position.X, (int)p.Position.Y + i].Inside = null;
			}
			for (int i = -1; p.Position.Y + i > 0 && Map[(int)p.Position.X, (int)p.Position.Y + i].IsWalkable; i--)
			{
				Map[(int)p.Position.X, (int)p.Position.Y + i].HasPellet = false;
				Map[(int)p.Position.X, (int)p.Position.Y + i].Inside = null;
			}
		}
		// to save visible pellets into grid
		foreach (Pellet p in pellets)
		{
			Map[(int)p.Position.X, (int)p.Position.Y].Inside = p;
			Map[(int)p.Position.X, (int)p.Position.Y].HasPellet = true;
			Map[(int)p.Position.X, (int)p.Position.Y].IsVisiblePellet = true;

		}
		// To save visible pacman in grid
		foreach (Pac p in pacs)
		{
			Map[(int)p.Position.X, (int)p.Position.Y].HasPellet = false;
			Map[(int)p.Position.X, (int)p.Position.Y].Inside = p;
			if (p.Mine)
				Map[(int)p.Position.X, (int)p.Position.Y].HasMyPac = true;
			else
				Map[(int)p.Position.X, (int)p.Position.Y].HasOpponentPac = true;
		}
	}

	public override string ToString()
	{
		string result = "";

		for (int i = 0; i < this.Height; i++)
		{
			for (int j = 0; j < this.Width; j++)
			{
				if (this.Map[j,i].HasPellet)
				{
					if (this.Map[j,i].Inside is Pellet pel && pel.Value == 10)
						result += "*";
					else
						result += "-";
				}
				else if (this.Map[j, i].Inside != null)
				{
					if (this.Map[j,i].Inside is Pac pa)
						result += pa.Id;
					else
						result += "!";
				}
				else
					result += this.Map[j, i].IsWalkable ? " " : "#";			
			}
			result += "\n";
		}
		return result;
	}
	public List<Vector2> GetWalkableNeibours(Vector2 v)
	{
		List<Vector2> result = new List<Vector2>();

		//                         Left, Right, Down,  Up,
		int[] dirRow = new int[4] {  -1,    +1,    0,   0};
		int[] dirCol = new int[4] {   0,     0,   +1,  -1};

		for (int i = 0; i < 4; i++)
		{
			int rr = (int)v.X + dirRow[i];
			int cc = (int)v.Y + dirCol[i];

			if (rr < 0)
				result.Add(new Vector2(Width - 1, cc)); // tunnel
			else if (rr == this.Width)
				result.Add(new Vector2(0, cc)); 		// tunnel
			else if (this.Map[rr,cc].IsWalkable)
				result.Add(new Vector2(rr, cc));
	
		}
		return (result);
	}
	public List<Vector2> GetWalkableNeiboursPacsAsWall(Vector2 v)
	{
		List<Vector2> result = new List<Vector2>();

		//                         Left, Right, Down,  Up,
		int[] dirRow = new int[4] {  -1,    +1,    0,   0};
		int[] dirCol = new int[4] {   0,     0,   +1,  -1};

		for (int i = 0; i < 4; i++)
		{
			int rr = (int)v.X + dirRow[i];
			int cc = (int)v.Y + dirCol[i];

			if (rr < 0)
			{
				if (!(this.Map[Width - 1, cc].Inside is Pac))
					result.Add(new Vector2(Width - 1, cc));            // tunnel
			}
			else if (rr == this.Width)
			{
				if (!(this.Map[0, cc].Inside is Pac))
					result.Add(new Vector2(0, cc)); 		            // tunnel
			}
			else if (this.Map[rr,cc].IsWalkable && !(this.Map[rr, cc].Inside is Pac))
				result.Add(new Vector2(rr, cc));
		}
		return (result);
	}
	public List<Vector2> GetNeiboursWhere(Vector2 v, Predicate<Cell> predicate)
	{
		List<Vector2> result = new List<Vector2>();

		//                         Left, Right, Down,  Up,
		int[] dirRow = new int[4] {  -1,    +1,    0,   0};
		int[] dirCol = new int[4] {   0,     0,   +1,  -1};

		for (int i = 0; i < 4; i++)
		{
			int rr = (int)v.X + dirRow[i];
			int cc = (int)v.Y + dirCol[i];

			if (rr < 0)
			{
				if (predicate(this.Map[Width - 1, cc]))
					result.Add(new Vector2(Width - 1, cc));            // tunnel
			}
			else if (rr == this.Width)
			{
				if (predicate(this.Map[0, cc]))
					result.Add(new Vector2(0, cc)); 		            // tunnel
			}
			else if (predicate(this.Map[rr,cc]))
				result.Add(new Vector2(rr, cc));
		}
		return (result);
	}
}

public class Game{
	public Grid Grid{get;set;}
	public int MyScore{get;set;}
	public int OpponentScore{get;set;}
	public int VisiblePacCount{get;set;}
	public List<Pac> Pacs{get;set;}
	public int VisiblePelletCount{get;set;}
	public List<Pellet> Pellets{get;set;}
	public Stopwatch StopWatch{get;set;}
	public ushort Turn{get;set;}

	public Game()
	{
		string[] inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]); // size of the grid
        int height = int.Parse(inputs[1]); // top left corner is (x=0, y=0)
		this.Grid = new Grid(width,height);
		this.Pacs = new List<Pac>(10); // 10 pacman max (5 by team)
		this.StopWatch = new Stopwatch();
		this.Turn = 0;
	}
	public void Sync()
	{
		string[] inputs = Console.ReadLine().Split(' ');
		this.StopWatch.Restart();
		this.MyScore = int.Parse(inputs[0]);
		this.OpponentScore = int.Parse(inputs[1]);
		
		// Reset is alive to false foreach pac
		foreach (Pac p in GetMyPacs())
			p.IsAlive = false;
		/// LOOP PACS
		this.VisiblePacCount = int.Parse(Console.ReadLine()); // all your pacs and enemy pacs in sight		

		GetOpponentPacs().ForEach(o => o.IsVisible = false); // reset visible flag for opponents
		for (int i = 0; i < this.VisiblePacCount; i++)
		{
			inputs = Console.ReadLine().Split(' ');
			int pacId = int.Parse(inputs[0]); // pac number (unique within a team)
			bool mine = inputs[1] != "0"; // true if this pac is yours
			Vector2 position = new Vector2(int.Parse(inputs[2]), int.Parse(inputs[3])); // position in the grid
			string typeId = inputs[4]; // unused in wood leagues
			int speedTurnsLeft = int.Parse(inputs[5]); // unused in wood leagues
			int abilityCooldown = int.Parse(inputs[6]); // unused in wood leagues

			Pac pac = Pacs.Find(e => e.Id == pacId && e.Mine == mine);
			if (pac == null)
				this.Pacs.Add(new Pac(pacId, mine, this.Turn, position, typeId, speedTurnsLeft, abilityCooldown));
			else
				pac.Update(this.Turn, position, typeId, speedTurnsLeft, abilityCooldown);	
		}

		// LOOP PELLETS
		this.VisiblePelletCount = int.Parse(Console.ReadLine()); // all pellets in sight
		this.Pellets = new List<Pellet>(this.VisiblePelletCount);

		for (int i = 0; i < this.VisiblePelletCount; i++)
		{
			inputs = Console.ReadLine().Split(' ');

			Vector2 position = new Vector2(int.Parse(inputs[0]), int.Parse(inputs[1]));
			int value = int.Parse(inputs[2]); // amount of points this pellet is worth
			this.Pellets.Add(new Pellet(position, value));
		}
		this.Grid.Update(this.Pacs, this.Pellets);
	}
	public static void Debug(string message)
	{
		Console.Error.WriteLine(message);
	}
	public List<Pac> GetMyPacs()
	{
		return this.Pacs.Where(e => e.Mine && e.IsAlive).OrderBy(e => e.Id).ToList();
	}
	public List<Pac> GetOpponentPacs()
	{
		return this.Pacs.Where(e => !e.Mine && e.IsAlive).OrderBy(e => e.Id).ToList();
	}
	public List<Pellet> GetPelletsNearest(Entity p_e)
	{
		return this.Pellets.OrderBy(e => e.Distance(this, p_e)).ToList();
	}
	public List<Pellet> GetPelletsNearest(Vector2 p_v)
	{
		return this.Pellets.OrderBy(e => e.Distance(this, p_v)).ToList();
	}
	public bool IsDirectPath(Entity e1, Entity e2)
	{
		if (BfsDepthWhere(e1.Position,c => c.IsVisiblePellet && c.IsWalkable  && !c.HasOpponentPac && !c.HasMyPac , c => c.Position == e2.Position) > 0)
			return true;
		return false;
		// int min;
		// int max;

		// if (e1.Position.X == e2.Position.X)
		// {
		// 	min = (int)Math.Min(e1.Position.Y, e2.Position.Y);
		// 	max = (int)Math.Max(e1.Position.Y, e2.Position.Y);
		// 	while (++min < max)
		// 		if (
		// 			!Grid.Map[(int)e1.Position.X, min].IsWalkable 
		// 			|| (Grid.Map[(int)e1.Position.X, min].Inside is Pac p1 && p1.Mine)
		// 		)
		// 			return false;
		// 	return true;
		// }
		// else if (e1.Position.Y == e2.Position.Y)
		// {
		// 	min = (int)Math.Min(e1.Position.X, e2.Position.X);
		// 	max = (int)Math.Max(e1.Position.X, e2.Position.X);
		// 	while (++min < max)
		// 		if (
		// 			!Grid.Map[min, (int)e1.Position.Y].IsWalkable 
		// 			|| (Grid.Map[min, (int)e1.Position.Y].Inside is Pac p1 && p1.Mine)
		// 		)
		// 			return false;
		// 	return true;
		// }
		// else
		// 	return false;
	}
	public List<Pellet> GetVisiblePelletsNearest(Entity p_e)
	{
		return this.Pellets.Where(e => this.IsDirectPath(e, p_e) ).OrderBy(e => e.Distance(this, p_e)).ToList();
	}
	public List<Vector2> BfsPathWhere(Vector2 start, Predicate<Cell> neighboursPredicate, Predicate<Cell> endPredicate)
	{
		bool found = false;
		Queue<Vector2> q = new Queue<Vector2>();
		q.Enqueue(start);

		bool[,] visited = new bool[Grid.Width,Grid.Height];
		visited[(int)start.X, (int)start.Y] = true;

		Stack<(Vector2 actual, Vector2 prev)> lookup = new Stack<(Vector2, Vector2)>();
		while (q.Any())
		{
			Vector2 node = q.Dequeue();
			List<Vector2> neighbours = Grid.GetNeiboursWhere(node, neighboursPredicate);

			foreach (Vector2 v in neighbours)
			{
				if (visited[(int)v.X, (int)v.Y])
					continue;
				q.Enqueue(v);
				visited[(int)v.X, (int)v.Y] = true;
				lookup.Push((v, node));
				if (endPredicate(this.Grid.Map[(int)v.X, (int)v.Y]))
				{
					found = true;
					goto End;
				}
			}
		}
		End:
		List<Vector2> result = new List<Vector2>();
		if (!found)
			return (result);
		var tmp = lookup.Pop();
	 	Vector2 prev = tmp.prev;
	 	result.Add(tmp.actual);
		while(lookup.Any())
		{
			while (tmp.actual != prev && lookup.Any())
				tmp = lookup.Pop();
			result.Insert(0, tmp.actual);
			prev = tmp.prev;
		}
		return (result);
	}

	void ExecuteActions()
	{
		GetMyPacs().ForEach(p => p.ExecuteAction());
		Console.WriteLine();
		Debug($"{this.StopWatch.ElapsedMilliseconds}ms");
		this.Turn++;
	}
	Vector2 SPEED_GetNearestPelletFromPellet(Vector2 p)
	{
		//LEFT FIRST
		if (p.X > 0 &&  Grid.Map[(int)p.X - 1, (int)p.Y].HasPellet)
				return (Grid.Map[(int)p.X - 1, (int)p.Y].Position);
		//RIGHT
		if (p.X < Grid.Width - 1 && Grid.Map[(int)p.X + 1, (int)p.Y].HasPellet)
						return (Grid.Map[(int)p.X + 1, (int)p.Y].Position);
		//UP
		if (p.Y > 0 && Grid.Map[(int)p.X, (int)p.Y - 1].HasPellet)
			   return (Grid.Map[(int)p.X, (int)p.Y - 1].Position);
		//DOWN
		if (p.Y < Grid.Height - 1 && Grid.Map[(int)p.X, (int)p.Y + 1].HasPellet)
		 		         return (Grid.Map[(int)p.X, (int)p.Y + 1].Position);
		return (p);
	}
	
	public int BfsDepthWhere(Vector2 start, Predicate<Cell> neighboursPredicate, Predicate<Cell> endPredicate)
	{
		Queue<(Vector2, int)> q = new Queue<(Vector2, int)>();
		q.Enqueue((start, 0));

		bool[,] visited = new bool[Grid.Width,Grid.Height];
		visited[(int)start.X, (int)start.Y] = true;

		if (endPredicate(this.Grid.Map[(int)start.X, (int)start.Y]))
			return 0;
		while (q.Any())
		{
			(Vector2 node, int depth) = q.Dequeue();
			List<Vector2> neighbours = Grid.GetNeiboursWhere(node, neighboursPredicate);
			foreach (Vector2 v in neighbours)
			{
				if (visited[(int)v.X, (int)v.Y])
					continue;
				if (endPredicate(this.Grid.Map[(int)v.X, (int)v.Y]))
					return(depth + 1);
				q.Enqueue((v, depth + 1));
				visited[(int)v.X, (int)v.Y] = true;
			}
		}
		return (-1);
	}
	public int BfsDepth(Vector2 start, Vector2 end)
	{
		Queue<(Vector2, int)> q = new Queue<(Vector2, int)>();
		q.Enqueue((start, 0));

		bool[,] visited = new bool[Grid.Width,Grid.Height];
		visited[(int)start.X, (int)start.Y] = true;

		while (q.Any())
		{
			(Vector2 node, int depth) = q.Dequeue();
			List<Vector2> neighbours = Grid.GetWalkableNeibours(node);

			foreach (Vector2 v in neighbours)
			{
				if (visited[(int)v.X, (int)v.Y])
					continue;
				if (v == end)
					return(depth + 1);
				q.Enqueue((v, depth + 1));
				visited[(int)v.X, (int)v.Y] = true;
			}
		}
		return (0);
	}

	public int BfsDepthPacsAsWall(Vector2 start, Vector2 end)
	{
		Queue<(Vector2, int)> q = new Queue<(Vector2, int)>();
		q.Enqueue((start, 0));

		bool[,] visited = new bool[Grid.Width,Grid.Height];
		visited[(int)start.X, (int)start.Y] = true;

		while (q.Any())
		{
			(Vector2 node, int depth) = q.Dequeue();
			List<Vector2> neighbours = Grid.GetWalkableNeiboursPacsAsWall(node);

			foreach (Vector2 v in neighbours)
			{
				if (visited[(int)v.X, (int)v.Y])
					continue;
				if (v == end)
					return(depth + 1);
				q.Enqueue((v, depth + 1));
				visited[(int)v.X, (int)v.Y] = true;
			}
		}
		return (0);
	}
	public List<Vector2> BfsPath(Vector2 start, Vector2 end)
	{
		bool found = false;
		Queue<Vector2> q = new Queue<Vector2>();
		q.Enqueue(start);

		bool[,] visited = new bool[Grid.Width,Grid.Height];
		visited[(int)start.X, (int)start.Y] = true;

		Stack<(Vector2 actual, Vector2 prev)> lookup = new Stack<(Vector2, Vector2)>();
		while (q.Any())
		{
			Vector2 node = q.Dequeue();
			List<Vector2> neighbours = Grid.GetNeiboursWhere(node, n => n.IsWalkable);

			foreach (Vector2 v in neighbours)
			{
				if (visited[(int)v.X, (int)v.Y])
					continue;
				q.Enqueue(v);
				visited[(int)v.X, (int)v.Y] = true;
				lookup.Push((v, node));
				if (v == end)
				{
					found = true;
					goto End;
				}
			}
		}
		End:
		List<Vector2> result = new List<Vector2>();
		if (!found)
			return result;
		var tmp = lookup.Pop();
	 	Vector2 prev = tmp.prev;
	 	result.Add(tmp.actual);
		while(lookup.Any())
		{
			while (tmp.actual != prev && lookup.Any())
				tmp = lookup.Pop();
			if (tmp.actual == prev)
				result.Insert(0, tmp.actual);
			prev = tmp.prev;
		}
		return (result);
	}

	public List<Vector2> BfsPathNearestPelletPacAsWall(Vector2 start)
	{
		Queue<Vector2> q = new Queue<Vector2>();
		q.Enqueue(start);

		bool[,] visited = new bool[Grid.Width,Grid.Height];
		visited[(int)start.X, (int)start.Y] = true;

		Stack<(Vector2 actual, Vector2 prev)> lookup = new Stack<(Vector2, Vector2)>();
		while (q.Any())
		{
			Vector2 node = q.Dequeue();
			List<Vector2> neighbours = Grid.GetWalkableNeiboursPacsAsWall(node);

			foreach (Vector2 v in neighbours)
			{
				if (visited[(int)v.X, (int)v.Y])
					continue;
				q.Enqueue(v);
				visited[(int)v.X, (int)v.Y] = true;
				lookup.Push((v, node));
				if (Grid.Map[(int)v.X, (int)v.Y].HasPellet 
					&& Grid.GetNeiboursWhere(v, c => !(c.Inside is Pac po && !po.Mine )).Any())
					goto End;
			}
		}
		End:
		if (!lookup.Any())
			return (new List<Vector2>());
		List<Vector2> result = new List<Vector2>();
		
		var tmp = lookup.Pop();
	 	Vector2 prev = tmp.prev;
	 	result.Add(tmp.actual);
		while(lookup.Any())
		{
			while (tmp.actual != prev && lookup.Any())
				tmp = lookup.Pop();
			if (tmp.actual == prev)
				result.Insert(0, tmp.actual);
			prev = tmp.prev;
		}
		return (result);
	}

	// public void Dfs(Vector2 start, Vector2 end)
	// {

	// }


	public void Play()
	{
		// reset target pellet if pellet not target not exist anymore
		foreach (Pac pa in GetMyPacs().Where(p => p.Action.HasAction).ToList())
				pa.Action = new Action();

		// IF there is BIG pellets, go get them
		foreach (Pellet pe in Pellets.Where(p => p.Value == 10).ToList())
		{
			Pac pa = GetMyPacs().OrderBy(p => p.Distance(this, pe)).First();

			if (!pa.Action.HasAction || pe.Distance(this, pa) < pa.Distance(this, pa.Action.TargetPosition))
				pa.Move(pe);
		}

		//Else set the nearest not targeted pellet
		foreach (Pac p in GetMyPacs().Where(p => !p.Action.HasAction).ToList())
		{
			List<Pellet>  AlreadyTargeted = GetMyPacs().Select(e => e.Action.TargetEntity).Where(e => e != null).Cast<Pellet>().ToList();
			p.Move(GetVisiblePelletsNearest(p).Except(AlreadyTargeted).FirstOrDefault());
			if (!p.Action.HasAction) // If can't see pellets go where we never go
			{
				var target = BfsPathWhere(p.Position, c => c.IsWalkable, c => c.HasPellet);
				if (target.Any())
					p.Move(target.Last());
			}
		}

		// If speed, be sure to target the second nearest pellet from the first one 
		foreach (Pac p in GetMyPacs().FindAll(p => p.SpeedTurnsLeft > 0))
		 	if (p.Distance(this, p.Action.TargetPosition) <= 2)
		 		p.Move(SPEED_GetNearestPelletFromPellet(p.Action.TargetPosition));
		
		
		// If can speed and no big pellet nearest, Override and speed
		GetMyPacs().FindAll(p => p.AbilityCooldown == 0 
			&& !(p.Action.TargetEntity is Pellet pel && pel.Value == 10 && pel.Distance(this, p) < 2)).ForEach(p => p.Speed());
		
		//Avoid colision
		bool[,] positions = new bool[Grid.Width, Grid.Height];
		foreach (Pac p in GetMyPacs())
		{
			if (p.Action.IsMove)
			{
				List<Vector2> path = BfsPath(p.Position, p.Action.TargetPosition); 
				if (!path.Any())
				{					
					positions[(int)path.First().X, (int)path.First().Y] = true;
					continue;
				}
				bool hasPacInIt = positions[(int)path.First().X, (int)path.First().Y];			
				if (hasPacInIt)
				{
					p.Move(p.Position);
					p.Label = "Stuck HERE";
					continue;
				}
				else
				{
					positions[(int)path.First().X, (int)path.First().Y] = true;
				}
			}
			else
				positions[(int)p.Position.X, (int)p.Position.Y] = true;
		}

		// Logic with opponent pac
		foreach (Pac p in GetMyPacs())
		{
			List<Pac> opponents = GetOpponentPacs().Where(o => o.IsVisible && o.Distance(this , p) <= o.SpeedStep + p.SpeedStep).OrderBy(o => o.Distance(this , p)).ToList();

			if (!opponents.Any())
				continue;
			if (p.CanBeEatenBy(opponents.First()) || opponents.First().AbilityCooldown == 0)
			{
				if (p.CanBeEatenBy(opponents.First()) && p.AbilityCooldown == 0)
					p.SwitchToEat(opponents.First());
				else if (p.Distance(this, p.Action.TargetPosition) >= opponents.First().Distance(this, p.Action.TargetPosition))// find another path
				{	
					
					var path = BfsPathNearestPelletPacAsWall(p.Position);
					if (path.Any())
						p.Move(path.Count() > 1 ? path.Skip(1).First() : path.First());
					else
					{
						p.Move(p);
						p.Label = "STUCK !";
					}
				}
			}
			else if (!p.CanBeEatenBy(opponents.First()) && !p.CanEat(opponents.First()))
			{
				if (p.AbilityCooldown == 0 && opponents.First().Distance(this, p) <= 2)
					p.SwitchToEat(opponents.First());
			}
		}

		

		//Say Hi
		foreach (Pac p in GetMyPacs())
		{
			if (GetOpponentPacs().Any(o => this.IsDirectPath(p, o)) && p.Label == null)
				p.Label = "Hi !";
		}

		this.ExecuteActions();
	}
}