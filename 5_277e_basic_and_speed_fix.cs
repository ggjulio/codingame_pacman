using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;


/**
 * Grab the pellets as fast as you can!
 **/
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
	public float Distance(Entity e)
	{
		return(Vector2.Distance(Position, e.Position));
	}
	public float Distance(Vector2 v)
	{
		return(Vector2.Distance(Position, v));
	}
	public override string ToString()
	{
		return ($"Entity(Position:{Position})");
	}
}
public enum eSwitch{
	Rock,
	Paper,
	Scissors,
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
	public List<Vector2> Positions{get;set;}
	public string TypeId{get;set;}
	public int SpeedTurnsLeft{get;set;}
	public int AbilityCooldown{get;set;}
	public bool	IsAlive{get;set;}
	public string Label{get;set;}
	public Action Action{get;set;}

	public Pac(int id, bool mine, Vector2 position, string typeId,
		int speedTurnsLeft, int abilityCooldown) : base(position)
	{
		this.Id = id;
		this.Mine = mine;
		this.Positions = new List<Vector2>();
		this.Positions.Add(position);
		this.TypeId = typeId;
		this.SpeedTurnsLeft = speedTurnsLeft;
		this.AbilityCooldown = abilityCooldown;
		this.IsAlive = true;
		this.Action = new Action();
	}

	public void Update(Vector2 position, string typeId, int speedTurnsLeft, int abilityCooldown)
	{
		base.Update(position);
		Positions.Add(position);
		this.TypeId = typeId;
		this.SpeedTurnsLeft = speedTurnsLeft;
		this.AbilityCooldown = abilityCooldown;
		this.IsAlive = true;
		this.Label = "";
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
	}
	public override string ToString()
	{
		return $"Pac(Id:{Id};Mine:{Mine};Position:{Position};SpeedTurnsLeft:{SpeedTurnsLeft};AbilityCooldown:{AbilityCooldown})";
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
			for (int i = -1; p.Position.X + i > 0 && Map[(int)p.Position.X + i, (int)p.Position.Y].IsWalkable; i--)
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
			Map[(int)p.Position.X, (int)p.Position.Y].Inside = p;
			Map[(int)p.Position.X, (int)p.Position.Y].HasPellet = false;
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

	public Game()
	{
		string[] inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]); // size of the grid
        int height = int.Parse(inputs[1]); // top left corner is (x=0, y=0)
		this.Grid = new Grid(width,height);
		this.Pacs = new List<Pac>(10); // 10 pacman max (5 by team)
		this.StopWatch = new Stopwatch();
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
				this.Pacs.Add(new Pac(pacId, mine, position, typeId, speedTurnsLeft, abilityCooldown));
			else
				pac.Update(position, typeId, speedTurnsLeft, abilityCooldown);	
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
		return this.Pellets.OrderBy(e => e.Distance(p_e)).ToList();
	}
	public List<Pellet> GetPelletsNearest(Vector2 p_v)
	{
		return this.Pellets.OrderBy(e => e.Distance(p_v)).ToList();
	}
	public bool IsDirectPath(Entity e1, Entity e2)
	{
		int min;
		int max;

		if (e1.Position.X == e2.Position.X)
		{
			min = (int)Math.Min(e1.Position.Y, e2.Position.Y);
			max = (int)Math.Max(e1.Position.Y, e2.Position.Y);
			while (++min < max)
				if (
					!Grid.Map[(int)e1.Position.X, min].IsWalkable 
					|| (Grid.Map[(int)e1.Position.X, min].Inside is Pac p1 && p1.Mine)
				)
					return false;
			return true;
		}
		else if (e1.Position.Y == e2.Position.Y)
		{
			min = (int)Math.Min(e1.Position.X, e2.Position.X);
			max = (int)Math.Max(e1.Position.X, e2.Position.X);
			while (++min < max)
				if (
					!Grid.Map[min, (int)e1.Position.Y].IsWalkable 
					|| (Grid.Map[min, (int)e1.Position.Y].Inside is Pac p1 && p1.Mine)
				)
					return false;
			return true;
		}
		else
			return false;
	}
	public List<Pellet> GetVisiblePelletsNearest(Entity p_e)
	{
		return this.Pellets.Where(e => this.IsDirectPath(e, p_e) ).OrderBy(e => e.Distance(p_e)).ToList();
	}

	void ExecuteActions()
	{
		GetMyPacs().ForEach(p => p.ExecuteAction());
		Console.WriteLine();
		//Debug($"{this.StopWatch.ElapsedMilliseconds}ms");
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

	public void Play()
	{
		Debug(Grid.ToString());
		Debug(Grid.Map[5,9].ToString());
		Debug(Grid.Map[13,3].ToString());
		// reset target pellet if pellet not target not exist anymore
		foreach (Pac pa in GetMyPacs().Where(p => p.Action.HasAction).ToList())
		{
			if (!Grid.Map[(int)pa.Action.TargetPosition.X, (int)pa.Action.TargetPosition.Y].HasPellet)
			{
				Debug($"YES ! {pa.Id} target{pa.Action.TargetPosition}");
				pa.Action = new Action();
			}
			else
				Debug($"NO  ! {pa.Id} target{pa.Action.TargetPosition}");
		}

		// IF there is BIG pellets, go get them
		List<Pellet> bigPellets = Pellets.Where(p => p.Value == 10).ToList();
		foreach (Pellet pe in bigPellets)
		{
			Pac pa = GetMyPacs().OrderBy(p => p.Distance(pe)).First();

			if (!pa.Action.HasAction || pe.Distance(pa) < pa.Distance(pa.Action.TargetPosition))
				pa.Move(pe);
		}

		// Else set the nearest not targeted pellet
		foreach (Pac p in GetMyPacs())
		{
			if (!p.Action.HasAction)
			{
				List<Pellet>  AlreadyTargeted = GetMyPacs().Select(e => e.Action.TargetEntity).Where(e => e != null).Cast<Pellet>().ToList();

				p.Move(GetVisiblePelletsNearest(p).Except(AlreadyTargeted).FirstOrDefault());

				///  PROBABLY BETTER WITHOUT WHEN SEVERAL PAC
				if (!p.Action.HasAction)
					p.Move(GetVisiblePelletsNearest(p).FirstOrDefault());
			}
			// If can't see pellets go where we never go
			if (!p.Action.HasAction)
			{
				List<Entity> AlreadyTargeted = GetMyPacs().Where(e => e.Id != p.Id).Select(e => e.Action.TargetEntity).ToList();

//				Cell target = Grid.Map.Cast<Cell>().Where(e => e.HasPellet).OrderBy(e => p.Distance(e.Position)).FirstOrDefault();
				var  target = Grid.Map.Cast<Cell>().Where(e => e.HasPellet).Select(e => e.Inside).Cast<Pellet>()
										.Except(AlreadyTargeted).OrderBy(e => p.Distance(e.Position)).FirstOrDefault();
				if (target != null)
					p.Move(target.Position);
			}
		}

		// If speed, be sure to target the second nearest pellet from the first one 
		foreach (Pac p in GetMyPacs().FindAll(p => p.SpeedTurnsLeft > 0))
		 	if (p.Distance(p.Action.TargetPosition) <= 1)
		 		p.Move(SPEED_GetNearestPelletFromPellet(p.Action.TargetPosition));
		
		
		// If can speed, Override and speed
		GetMyPacs().FindAll(p => p.AbilityCooldown == 0).ForEach(p => p.Speed());
		
		//Say Hi
		foreach (Pac p in GetMyPacs())
		{
			if (GetOpponentPacs().Any(o => this.IsDirectPath(p, o)))
				p.Label = "Hi !";
		}

		this.ExecuteActions();
	}
}