using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;



/**
 * Grab Snaffles and try to throw them through the opponent's goal!
 * Move towards a Snaffle to grab it and use your team id to determine towards where you need to throw it.
 * Use the Wingardium spell to move things around at your leisure, the more magic you put it, the further they'll move.
 **/

static class Constants
{
	public const int wingardiumShootAmount = 35; // 15 last submit

}

class Player
{
	static void Main(string[] args)
	{
		Game        game;

		// Readline return my team id, and instanciate game
		game = new Game(int.Parse(Console.ReadLine()));

		// game loop
		while (true)
		{
			game.SyncGame();
			game.Play();
		}
	}
}

public static class Collision
{
	public static bool CircleCircle(int radiusC1, int radiusC2, int distance)
	{
		return (distance <= radiusC1 + radiusC2);
	}

	public static bool LineCircle( Vector2 lineFrom, Vector2 lineTo, Vector2 circle, float radius)
    {
        float ab2, acab, h2;

        Vector2 ac = circle - lineFrom;
        Vector2 ab = lineTo - lineFrom;
        ab2 = Vector2.Dot(ab,  ab);
        acab = Vector2.Dot(ac, ab);
       
	    float t = acab / ab2;
        if (t < 0)
            t = 0;
        else if (t > 1)
            t = 1;

        Vector2 h = ((ab * t) + lineFrom) - circle;
        h2 = Vector2.Dot(h, h);

        return (h2 <= (radius * radius));
    }
}

public abstract class Entity
{        
	public int 		Id {get;}
	public string	Type {get;}
	public List<Vector2> Positions{get; set;}		
	public Vector2 ActualPosition{get; set;}		
	public List<Vector2> Velocities {get; set;}
	public Vector2 ActualVelocity {get; set;}
	public abstract double	Mass {get;}
	public abstract double	Friction {get;}
	public abstract int		Radius {get;}

	public Entity(int id, string type, Vector2 position, Vector2 velocity)
	{
		Id = id;
		Type = type;
		Positions = new List<Vector2>();
		Positions.Add(position);
		ActualPosition = position;
		Velocities = new List<Vector2>();
		Velocities.Add(velocity);
		ActualVelocity = velocity;
	}
	public void Update(Vector2 position, Vector2 velocity)
	{
		Positions.Add(position);
		ActualPosition = position;
		Velocities.Add(velocity);
		ActualVelocity = velocity;
	}
	public float Distance(Entity e)
	{
		return(Vector2.Distance(ActualPosition, e.ActualPosition));
	}
	public float Distance(Vector2 v)
	{
		return(Vector2.Distance(ActualPosition, v));
	}
	public double AngleBetween(Entity e)
	{
		Vector2 v1 = ActualPosition;
		Vector2 v2 = e.ActualPosition;

		return (Math.Atan2(v2.Y - v1.Y, v2.X - v1.X) * 180 / Math.PI);
	}

	public override string ToString()
	{
		return ($"Entity(id:{Id};Position:{ActualPosition};Velocity:{Velocities.Last()})");
	}
}

public class Wizard : Entity
{
	public bool	CanShootSnaffle {get; set;}
	public bool HasGrabbedSnaffle {get; set;}
	public Snaffle			grabbedSnaffle {get; set;}
	public List<Snaffle>	NearestSnaffles {get; set;}
	public Snaffle			TargetSnaffle {get; set;}
	public override double	Mass{get {return 1.0;}}
	public override double	Friction {get {return 0.75;}}
	public override int		Radius {get {return 400;}}
	public eAction			Action{get; set;}

	public Wizard(int id, string type, Vector2 position, Vector2 velocity, bool canShootSnaffle) : base(id, type, position, velocity)
	{
		CanShootSnaffle = canShootSnaffle;
		HasGrabbedSnaffle = false;
		Action = eAction.None;
		NearestSnaffles = null;
		TargetSnaffle = null;
	}

	public void Update(Vector2 position, Vector2 velocity, bool canShootSnaffle)
	{
		base.Update(position, velocity);
		CanShootSnaffle = canShootSnaffle;
		HasGrabbedSnaffle = false;
		grabbedSnaffle = null;
		Action = eAction.None;
		NearestSnaffles = null;
		TargetSnaffle = null;
	}	
	public void Move(Vector2 targetPosition, int thrust)
	{
		Console.WriteLine($"MOVE {targetPosition.X} {targetPosition.Y} {thrust}"); 
	}
	public void Shoot(Vector2 targetPosition, int power)
	{
		Console.WriteLine($"THROW {targetPosition.X} {targetPosition.Y} {power}"); 
	}
	public void Wingardium(Entity entity, Vector2 targetPosition, int magic)
	{
		Console.WriteLine($"WINGARDIUM {entity.Id} {targetPosition.X} {targetPosition.Y} {magic}"); 
	}

	public override string ToString()
	{
		return ($"Wizard(id:{Id};Position:{ActualPosition};Velocity:{Velocities.Last()};CanShootSnaffle:{CanShootSnaffle})");
	}
}

public class Snaffle : Entity
{
	public bool StillExist {get; set;}
	public bool	CanBeShooted {get; set;}
	public bool HasBeenGrabbed {get; set;}  // the distance between snaffle and wizard is less than 400 unit.
	public Wizard Grabber {get; set;}
	public List<Wizard> NearestWizards {get; set;}
	public override double	Mass{get {return 0.5;}}
	public override double	Friction {get {return 0.75;}}
	public override int		Radius {get {return 150;}}
	
	public Snaffle(int id, string type, Vector2 position, Vector2 velocity, bool canBeShooted) : base(id, type, position, velocity)
	{
		CanBeShooted = canBeShooted;
		StillExist = true;
		HasBeenGrabbed = false;
		NearestWizards = null;
	}
	public void Update(Vector2 position, Vector2 velocity, bool canBeShooted)
	{
		base.Update(position, velocity);
		CanBeShooted = canBeShooted;
		StillExist = true;
		Grabber = null;
		HasBeenGrabbed = false;
		NearestWizards = null;
	}
	public override string ToString()
	{
		return ($"Snaffle(id:{Id};Position:{ActualPosition};Velocity:{Velocities.Last()};CanBeShooted:{CanBeShooted})");
	}
}

public class Bludger : Entity
{
	public int 		IdLastVictim {get; set;}
	
	public List<Wizard> NearestWizards {get; set;}
	public int				Thrust{get{ return 1000;}}
	public override double	Mass{get {return 8;}}
	public override double	Friction {get {return 0.9;}}
	public override int		Radius {get {return 200;}}

	public Bludger(int id, string type, Vector2 position, Vector2 velocity, int idLastVictim) : base(id, type, position, velocity)
	{
		IdLastVictim = idLastVictim;
		NearestWizards = null;
	}
	public void Update(Vector2 position, Vector2 velocity, int idLastVictim)
	{
		base.Update(position, velocity);
		IdLastVictim = idLastVictim;
		NearestWizards = null;

	}	
	public override string ToString()
	{
		return ($"Bludger(id:{Id};Position:{ActualPosition};Velocity:{Velocities.Last()};IdLastVictim:{IdLastVictim})");
	}
}

public class Team
{
	public int 			Id {get;}
	public Vector2      GoalCenterPosition {get;}
	public Vector2		Pole0 {get;}
	public Vector2		Pole1 {get;}

	public int          Score {get; set;}
	public int          Magic {get; set;}
	public int          MagicMax {get {return 100;}}

	public Team(int teamId, Vector2 goalCenterPosition, Vector2 pole0, Vector2 pole1)
	{
		Id = teamId;
		GoalCenterPosition = goalCenterPosition;
		Score = -1;
		Magic = -1;
		Pole0 = pole0;
		Pole1 = pole1;
	}
	public void Update(int score, int magic)
	{
		Score = score;
		Magic = magic;
	}	
}

public class Game
{
	public Team 			MyTeam {get; set;}
	public Team 			OpponentTeam {get; set;}
	public List<Entity>		Entities {get; set;}
	public static Vector2	MapSize {get {return new Vector2(16001, 7501);}}

	public int		DirToShoot {get;}
	
	// TeamId : 0 if MyGoal is on the left, 1 if MyGoal is on the right
	public Game(int myTeamId)
	{
		MyTeam = new Team(
			myTeamId,
			(myTeamId == 0 ? new Vector2(0, 3750) : new Vector2(16000, 3750)),
			(myTeamId == 0 ? new Vector2(0, 1750 + 451) : new Vector2(16000, 1750 + 451)),
			(myTeamId == 0 ? new Vector2(0, 5750 - 451) : new Vector2(16000, 5750 - 451))
		);
		OpponentTeam = new Team(
			myTeamId == 0 ? 1 : 0,
			(myTeamId == 0 ? new Vector2(16000, 3750) : new Vector2(0, 3750)),
			(myTeamId == 0 ? new Vector2(16000, 1750 + 451) : new Vector2(0, 1750 + 451)),
			(myTeamId == 0 ? new Vector2(16000, 5750 - 451) : new Vector2(0, 5750 - 451))
		);
		Entities = new List<Entity>();
		DirToShoot = myTeamId == 0 ? 1 : -1;
	}

	public void SyncGame()
	{
		string[] 	inputs;

		inputs = Console.ReadLine().Split(' ');
		MyTeam.Update(int.Parse(inputs[0]), int.Parse(inputs[1]));

		inputs = Console.ReadLine().Split(' ');
		OpponentTeam.Update(int.Parse(inputs[0]), int.Parse(inputs[1]));

		SetSnaffleStillExistToFalse();

		int nb_entities = int.Parse(Console.ReadLine()); // number of entities still in game
		for (int i = 0; i < nb_entities; i++)
		{
			inputs = Console.ReadLine().Split(' ');

			int 		entityId = int.Parse(inputs[0]);
			string		entityType = inputs[1]; // "WIZARD", "OPPONENT_WIZARD" or "SNAFFLE" or "BLUDGER"
			Vector2 	position = new Vector2(int.Parse(inputs[2]), int.Parse(inputs[3]));
			Vector2 	velocity = new Vector2(int.Parse(inputs[4]),int.Parse(inputs[5]));
			int 		state = int.Parse(inputs[6]);

			Entity actualElement = this.Entities.Find(e => e.Id == entityId);
			switch (entityType)
			{
				case "WIZARD":
					if (actualElement is Wizard e)
						e.Update(position, velocity, Convert.ToBoolean(state));
					else
						this.Entities.Add(
							new Wizard(entityId, entityType, position, velocity, 
										Convert.ToBoolean(state)));
					break;
				case "OPPONENT_WIZARD":
					if (actualElement is Wizard oe)
						oe.Update(position, velocity, Convert.ToBoolean(state));
					else
					this.Entities.Add(
						new Wizard(entityId, entityType, position, velocity, Convert.ToBoolean(state)));
					break;
				case "SNAFFLE":
					if (actualElement is Snaffle s)
						s.Update(position, velocity, Convert.ToBoolean(state));
					else
					this.Entities.Add(
						new Snaffle(entityId, entityType, position, velocity, Convert.ToBoolean(state)));
					break;
				case "BLUDGER":
					if (actualElement is Bludger b)
						b.Update(position, velocity, state);
					else
					this.Entities.Add(
						new Bludger(entityId, entityType, position, velocity, state));
					break;
				default:
					break;
			}
		}
		this.Entities = this.Entities.OrderBy(e => e.Id).ToList();
		ComputeAdditionalInfos();
	}
	private void ComputeAdditionalInfos()
	{	
		// Store grabbed snaffles in wizard object
		foreach (Wizard w in GetAllWizards())
			foreach (Snaffle s in GetSnaffles().Where(e => !e.HasBeenGrabbed).ToList())
			{
				if (Vector2.Distance(w.ActualPosition, s.ActualPosition) < 400)
				{
					w.HasGrabbedSnaffle = true;
					w.grabbedSnaffle = s;
					s.Grabber = w;
					s.HasBeenGrabbed = true;
					break;
				}
			}
		// Foreach wizard, save the list of his nearest snaffles
		foreach (Wizard w in GetAllWizards())
			w.NearestSnaffles = GetSnaffles().OrderBy(e => e.Distance(w)).ToList();	
		// Foreach snaffle, save the list of his nearest wizards
		foreach (Snaffle s in GetSnaffles())
			s.NearestWizards = GetAllWizards().OrderBy(e => e.Distance(s)).ToList();
		// Foreach bludger, save the list of his nearest wizards
		foreach (Bludger b in GetBludgers())
			b.NearestWizards = GetAllWizards().OrderBy(e => e.Distance(b)).ToList();
		// Compute TargetSnaffle based on Distance.
		// Should be the nearest, but if is the nearest of the two wizard, the one that is the furthest
	// will get another target
		SetTargetSnaffle(GetMyWizards());
		SetTargetSnaffle(GetOpponentWizards());
	}

	private void SetTargetSnaffle(List<Wizard> wizards)
	{
		Wizard w0 = wizards.First();
		Wizard w1 = wizards.Last();
		
		w0.TargetSnaffle = w0.NearestSnaffles.First();
		w1.TargetSnaffle = w1.NearestSnaffles.First();
		
		if (w0.TargetSnaffle.Id == w1.TargetSnaffle.Id)
		{
			//If wizard 0 is closer than wizard 1, assign the second nearest to targetSnaffle
			if (w0.Distance(w1.TargetSnaffle) < w1.Distance(w1.TargetSnaffle))
			{
				w1.TargetSnaffle = w1.NearestSnaffles.FirstOrDefault(e => e.Id != w0.TargetSnaffle.Id);
				if (w1.TargetSnaffle == null)
					w1.TargetSnaffle = w1.NearestSnaffles.FirstOrDefault();
			}
			else
			{
				w0.TargetSnaffle = w0.NearestSnaffles.FirstOrDefault(e => e.Id != w1.TargetSnaffle.Id);
				if (w0.TargetSnaffle == null)
					w0.TargetSnaffle = w0.NearestSnaffles.FirstOrDefault();
			}
		}
	}
	private void SetSnaffleStillExistToFalse()
	{
		foreach (Snaffle s in GetSnaffles())
			s.StillExist = false;
	}
	public List<Snaffle> GetSnaffles()
	{
		return (this.Entities.Where(e => e is Snaffle s && s.StillExist).Cast<Snaffle>().OrderBy(e => e.Id).ToList());
	}
	public List<Wizard> GetAllWizards()
	{
		return (GetMyWizards().Concat(GetOpponentWizards()).OrderBy(e => e.Id).ToList());
	}
	public List<Wizard> GetMyWizards()
	{
		return (this.Entities.Where(e => e is Wizard w && w.Type == "WIZARD").Cast<Wizard>().OrderBy(e => e.Id).ToList());
	}
	public List<Wizard> GetOpponentWizards()
	{
		return (this.Entities.Where(e => e is Wizard w && w.Type == "OPPONENT_WIZARD").Cast<Wizard>().OrderBy(e => e.Id).ToList());
	}
	public List<Bludger> GetBludgers()
	{
		return (this.Entities.Where(e => e is Bludger).Cast<Bludger>().OrderBy(e => e.Id).ToList());
	}
	public List<Entity> GetEntityBetween(Vector2 pointA, Vector2 pointB)
	{
		return this.Entities.FindAll(
			e => Collision.LineCircle(pointA, pointB, e.ActualPosition + e.ActualVelocity, e.Radius));
	}
	public List<Entity> GetCollidableEntityBetween(Vector2 pointA, Vector2 pointB)
	{
		return GetEntityBetween(pointA, pointB).FindAll(
			e => e.Type == "OPPONENT_WIZARD" || e is Bludger);
	}
	public int GetNbSnafflesToWin()
	{
		if (5 == Entities.Where(e => e is Snaffle).Count())
			return 3;
		return 4;	
	}

	public bool EntityBetween(Vector2 pointA, Vector2 pointB)
	{
		return this.Entities.Any(
			e => Collision.LineCircle(pointA, pointB, e.ActualPosition + e.ActualVelocity, e.Radius));
	}

	public bool CollidableEntityBetween(Vector2 pointA, Vector2 pointB)
	{
		return GetCollidableEntityBetween(pointA, pointB).Any();
	}

	public void Play()
	{
		//  (0 ≤ thrust ≤ 150, 0 ≤ power ≤ 500, 0 ≤ magic ≤ 1500)
		this.SetActions();
		// Do action foreach wizard in my team
		foreach (Wizard w in GetMyWizards().OrderBy(w => w.Id).ToList())
		{
			switch (w.Action)
			{
				case eAction.Shoot:
					Strategy.Shoot(this, w);
					break;
				case eAction.ShootWingardium:
					Strategy.ShootWingardium(this, w);
					break;
				case eAction.Attack:
					Strategy.Attack(this, w);
					break;
				case eAction.Defend:
					Strategy.Defend(this, w);
					break;
				case eAction.GetAnotherSnaffle:
					Strategy.GetAnotherSnaffle(this, w);
					break;
				case eAction.Gardian:
					Strategy.Gardian(this, w);
					break;
				case eAction.None:
				default:
					Game.Debug($"NO ACTION TAKEN by the Wizard id: {w.Id}");
					w.Move(new Vector2(0), 0);
					break;
			}
		}
	}
	public static void Debug(string message)
	{
		Console.Error.WriteLine(message);
	}

	private bool ShouldGetAnotherSnaffleBehind()
	{
		Wizard w = GetMyWizards().OrderBy(e => e.Distance(MyTeam.GoalCenterPosition)).First();
		

		// try put offset to anticipate we should get snaffle
		int nbSnafflesAhead = 
			GetSnaffles().Where(e => (w.grabbedSnaffle != null && w.grabbedSnaffle.Id == e.Id) || e.ActualPosition.X - 150 > w.ActualPosition.X * DirToShoot).Count();
		Game.Debug(nbSnafflesAhead.ToString());
		if (MyTeam.Score + nbSnafflesAhead < GetNbSnafflesToWin())
			return true;
		return false;
	}
	private void SetActions()
	{
// If a wizard own a snaffle --> shoot
		foreach (Wizard w in GetMyWizards())
			if (w.CanShootSnaffle)
				w.Action = eAction.Shoot;

//Check if should defend 
		// Wizard opp = GetOpponentWizards()
		// 	.Where(o => o.Distance(MyTeam.GoalCenterPosition) < Game.MapSize.X / 2 )
		// 	.OrderBy(o => o.Distance(MyTeam.GoalCenterPosition)).FirstOrDefault();
		
		// if (opp != null)
		// {
		// 	var w = GetMyWizards().OrderBy(e => e.Distance(opp)).ToList()
		// 		.Find(e => e.Action == eAction.None);
		// 	// Try to get a wizard that can't shoot !
		// 	if (w != null)
		// 		w.Action = eAction.Defend;
		// 	else
		// 		GetMyWizards().OrderBy(e => e.Distance(opp)).First().Action = eAction.Defend;
		// }
		
// Try wingardium shoot if magic > 40;
		if (MyTeam.Magic >= Constants.wingardiumShootAmount)
		{
			var w = GetMyWizards().Find(e => e.Action == eAction.None);
			// Try to get a wizard that can't shoot !
			if (w != null)
				w.Action = eAction.ShootWingardium;
			else
			GetMyWizards().First().Action = eAction.ShootWingardium;
		}

		if (ShouldGetAnotherSnaffleBehind())
		{
			var w = GetMyWizards().OrderBy(e => e.Distance(MyTeam.GoalCenterPosition)).First();
			
			w.Action = eAction.GetAnotherSnaffle;
		}

// If No action, attack
		foreach (Wizard w in GetMyWizards())
			if (w.Action == eAction.None)
				w.Action = eAction.Attack;
	}
}

public enum eAction
{
	None,
	Attack,
	GetAnotherSnaffle,
	Defend,
	Shoot,
	ShootWingardium,
	Gardian,
}

public class Strategy
{
	public static void GetAnotherSnaffle(Game game, Wizard wizard)
	{
		Snaffle s = wizard.NearestSnaffles.Where(e => e.ActualPosition.X < wizard.ActualPosition.X * game.DirToShoot).FirstOrDefault();
		Game.Debug("GetAnotherSnaffle !");

		if (s != null && game.MyTeam.Magic > 20)
			wizard.Wingardium(s, wizard.ActualPosition + wizard.ActualVelocity, game.MyTeam.Magic);
		else if (s != null)
			wizard.Move(s.ActualPosition + s.ActualVelocity, 150);
		else
		{
			s = wizard.NearestSnaffles.First();
			wizard.Move(s.ActualPosition + s.ActualVelocity, 150);
			Game.Debug("Get Another Snaffle DONT FIND SNAFFLE BEHIND !");
		}
	}

	public static void Attack(Game game, Wizard wizard)
	{
		wizard.Move(wizard.TargetSnaffle.ActualPosition + wizard.TargetSnaffle.ActualVelocity, 150);
	}

	public static void Shoot(Game game, Wizard wizard)
	{
		if (wizard.CanShootSnaffle)
			wizard.Shoot(BestDirectionToShoot(game, wizard.grabbedSnaffle), 500);
		else
			Game.Debug("Can't SHOOT BROO");
	}


	public static Vector2 BestDirectionToShoot(Game game, Snaffle toShoot)
	{
		List<Entity>	eBetween;

		Vector2 v = game.OpponentTeam.GoalCenterPosition;
		v.Y = toShoot.ActualPosition.Y;
		if (v.Y < game.OpponentTeam.Pole0.Y)      v.Y = game.OpponentTeam.Pole0.Y;
		else if (v.Y > game.OpponentTeam.Pole1.Y) v.Y = game.OpponentTeam.Pole1.Y;
		
		eBetween = game.GetCollidableEntityBetween(toShoot.ActualPosition + toShoot.ActualVelocity, v);
			
// Shoot to goal  //////////////////////////////////////////////////////////
		if ((!eBetween.Any() || !eBetween.Where(e => toShoot.Distance(e) < 8000).Any())
			&& toShoot.Distance(game.OpponentTeam.GoalCenterPosition) < 5000)
			return(game.OpponentTeam.GoalCenterPosition);
		else if (toShoot.Distance(game.OpponentTeam.GoalCenterPosition) < 4500)
		{
	
// Try random place between poles's goal  //////////////////////////////////////////////////////////
			Game.Debug ("Random Place between pole");

			Random r = new Random();
			for (int i = 0; i < 7; i++)
			{
				Vector2 rV = game.OpponentTeam.GoalCenterPosition;
				rV.Y = r.Next((int)game.OpponentTeam.Pole0.Y + 1 , (int)game.OpponentTeam.Pole1.Y);
				if (!game.CollidableEntityBetween(toShoot.ActualPosition  + toShoot.ActualVelocity, rV))
					return (rV);
			}
		}
//Try pass to nearest wizard  //////////////////////////////////////////////////////////
		// Game.Debug("HERRE");
		// Game.Debug("ddd.>" + toShoot.Id.ToString());
		// List<Wizard> w = game.GetMyWizards()
		// 	.Where(e => e.Id != toShoot.Grabber.Id && !game.CollidableEntityBetween(toShoot.ActualPosition + toShoot.ActualVelocity, e.ActualPosition + e.ActualVelocity))
		// 	.OrderBy(e => e.Distance(toShoot)).ToList();
		// Game.Debug("HERE");

		// if (w.Any())
		// 	Game.Debug(" ccc >" + (toShoot.ActualPosition.X - w.First().ActualPosition.X).ToString());
		// 	// if there is wizard AND if less than 2500 distance, and if not too far behind
		// if (w.Any() && toShoot.Distance(w.First().ActualPosition) < 5500 &&
		// 	toShoot.ActualPosition.X - w.First().ActualPosition.X < -300)
		// 	return(w.First().ActualPosition + w.First().ActualVelocity);

// Try to shoot to the next snaffle 
	//		
	//////////////////////////////////

// Try try all y possibility,and sort them to the nearest to the goal.	
	

	List<Vector2> vList = Enumerable.Repeat(new Vector2(game.OpponentTeam.Pole0.X), (int)(Game.MapSize.Y)).ToList();

	vList = vList.Select((e, i) => {e.Y = i; return e;}).ToList();

	vList = vList.Where(e => !game.CollidableEntityBetween(toShoot.ActualPosition, e) )
			.OrderBy(e => Vector2.Distance(e, game.OpponentTeam.GoalCenterPosition)).ToList();

	if (vList.Any())
		return (vList.First());



	Game.Debug("NO BEST SHOT FOUND : Default, goal.center shoot");
	return (game.OpponentTeam.GoalCenterPosition);


	}

	public static Vector2 BestDirectionWingardium(Game game, Snaffle toShoot)
	{
		List<Entity>	eBetween;

		Vector2 v = game.OpponentTeam.GoalCenterPosition;
		v.Y = toShoot.ActualPosition.Y;
		if (v.Y < game.OpponentTeam.Pole0.Y)      v.Y = game.OpponentTeam.Pole0.Y;
		else if (v.Y > game.OpponentTeam.Pole1.Y) v.Y = game.OpponentTeam.Pole1.Y;
		
		eBetween = game.GetCollidableEntityBetween(toShoot.ActualPosition + toShoot.ActualVelocity, v);
			
// Shoot to goal  //////////////////////////////////////////////////////////
		if ((!eBetween.Any() || !eBetween.Where(e => toShoot.Distance(e) < 8000).Any())
			&& toShoot.Distance(game.OpponentTeam.GoalCenterPosition) < 5000)
			return(game.OpponentTeam.GoalCenterPosition);
		else if (toShoot.Distance(game.OpponentTeam.GoalCenterPosition) < 4500)
		{
	
// Try random place between poles's goal  //////////////////////////////////////////////////////////
			Game.Debug ("Random Place between pole");

			Random r = new Random();
			for (int i = 0; i < 7; i++)
			{
				Vector2 rV = game.OpponentTeam.GoalCenterPosition;
				rV.Y = r.Next((int)game.OpponentTeam.Pole0.Y + 1 , (int)game.OpponentTeam.Pole1.Y);
				if (!game.CollidableEntityBetween(toShoot.ActualPosition  + toShoot.ActualVelocity, rV))
					return (rV);
			}
		}

// Try try all y possibility,and sort them to the nearest to the goal.	
	List<Vector2> vList = Enumerable.Repeat(new Vector2(game.OpponentTeam.Pole0.X), (int)(Game.MapSize.Y)).ToList();

	vList = vList.Select((e, i) => {e.Y = i; return e;}).ToList();

	vList = vList.Where(e => !game.CollidableEntityBetween(toShoot.ActualPosition, e) )
			.OrderBy(e => Vector2.Distance(e, game.OpponentTeam.GoalCenterPosition)).ToList();

	if (vList.Any())
		return (vList.First());



	Game.Debug("Wingardium NO BEST SHOT FOUND : Default, goal.center shoot");
	return (game.OpponentTeam.GoalCenterPosition);


	}


	public static void Gardian(Game game, Wizard wizard)
	{
		Vector2 v = game.MyTeam.GoalCenterPosition;
		if (v.X == 0)
			v.X += wizard.Radius;
		else
			v.X -= wizard.Radius;
		wizard.Move(v, 150);
	}


	public static void ShootWingardium(Game game, Wizard wizard)
	{
		Snaffle s = game.GetSnaffles()
			.OrderBy(e => e.Distance(game.OpponentTeam.GoalCenterPosition)).FirstOrDefault();

		int nbMagic = game.MyTeam.Magic / 3;
		if (s != null)
			wizard.Wingardium(s, BestDirectionWingardium(game, s), game.MyTeam.Magic);
		else
		{
			Strategy.Attack(game, wizard);
			Game.Debug("Can't wingardium shoot BROO ! ");
		}
	}

	public static void Defend(Game game, Wizard wizard)
	{
		List<Snaffle> sList = game.GetSnaffles().OrderBy(e => e.Distance(game.MyTeam.GoalCenterPosition)).ToList();

		Wizard otherW = game.GetMyWizards().Find(e => e.Id != wizard.Id);

		if (wizard.grabbedSnaffle != null && wizard.grabbedSnaffle.CanBeShooted)
			if (wizard.Distance(game.OpponentTeam.GoalCenterPosition) > otherW.Distance(game.OpponentTeam.GoalCenterPosition) && wizard.Distance(otherW) > 1000)
				wizard.Shoot(otherW.ActualPosition, 500);
			else
				wizard.Shoot(game.OpponentTeam.GoalCenterPosition, 500);
		else
			wizard.Move(sList.First().ActualPosition, 150);
	}












	static List<Snaffle> 	GetSnafflesInAngleRange(List<Snaffle> pSnaffles, Wizard wizard, double min, double max)
	{
		return (
			pSnaffles.Where(e => AngleInRange(wizard.AngleBetween(e), min, max)).ToList()
		);
	}
	public static bool AngleInRange(double angle, double min, double max)
	{
		if (angle >= min && angle <= max)
			return (true);
		return (false);
	}
}

