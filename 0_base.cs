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
	public List<Vector2> Positions{get;set;}
	public string TypeId{get;set;}
	public eSwitch Type{get;set;}
	public int SpeedTurnsLeft{get;set;}
	public int AbilityCooldown{get;set;}
	public bool	IsAlive{get;set;}
	public string Label{get;set;}
	public Action Action{get;set;}
	public Action PreviousAction{get;set;}
	public Pac(int id, bool mine, Vector2 position, string typeId,
		int speedTurnsLeft, int abilityCooldown) : base(position)
	{
		this.Id = id;
		this.Mine = mine;
		this.Positions = new List<Vector2>();
		this.Positions.Add(position);
		this.TypeId = typeId;
		this.Type = (eSwitch)Enum.Parse(typeof(eSwitch), typeId);
		this.SpeedTurnsLeft = speedTurnsLeft;
		this.AbilityCooldown = abilityCooldown;
		this.IsAlive = this.Type != eSwitch.DEAD ? true : false;
		this.Action = new Action();
	}
	public void Update(Vector2 position, string typeId, int speedTurnsLeft, int abilityCooldown)
	{
		base.Update(position);
		Positions.Add(position);
		this.TypeId = typeId;
		this.Type = (eSwitch)Enum.Parse(typeof(eSwitch), typeId);
		this.SpeedTurnsLeft = speedTurnsLeft;
		this.AbilityCooldown = abilityCooldown;
		this.IsAlive = this.Type != eSwitch.DEAD ? true : false;
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
		this.PreviousAction = this.Action;
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

public class Game{
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
		this.Pacs = new List<Pac>(10); // 10 pacman max (5 by team)
		this.StopWatch = new Stopwatch();
	}
	public void Sync()
	{
		string[] inputs = Console.ReadLine().Split(' ');
		this.StopWatch.Restart();
		this.MyScore = int.Parse(inputs[0]);
		this.OpponentScore = int.Parse(inputs[1]);
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
	void ExecuteActions()
	{
		GetMyPacs().ForEach(p => p.ExecuteAction());
		Console.WriteLine();
		//Debug($"{this.StopWatch.ElapsedMilliseconds}ms");
	}

	public void Play()
	{


		this.ExecuteActions();
	}
}