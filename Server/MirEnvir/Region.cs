using Server.MirDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.MirObjects
{
    public class Region
    {
        public List<Rank_Character_Info> RankTop = new List<Rank_Character_Info>();
        public List<Rank_Character_Info>[] RankClass = new List<Rank_Character_Info>[Enum.GetValues(typeof(MirClass)).Length];
        public int[] RankBottomLevel = new int[Enum.GetValues(typeof(MirClass)).Length + 1];

        int RankCount = 100;//could make this a global but it made sence since this is only used here, it should stay here

        public List<KeyValuePair<string, string>> GlobalVar = new List<KeyValuePair<string, string>>();

        public void Init()
        {
          //reset ranking
            for (int i = 0; i < RankClass.Count(); i++)
            {
                if (RankClass[i] != null)
                    RankClass[i].Clear();
                else
                    RankClass[i] = new List<Rank_Character_Info>();
            }
            RankTop.Clear();
            for (int i = 0; i < RankBottomLevel.Count(); i++)
            {
                RankBottomLevel[i] = 0;
            }

        }

        public int InsertRank(List<Rank_Character_Info> Ranking, Rank_Character_Info NewRank)
        {
            if (Ranking.Count == 0)
            {
                Ranking.Add(NewRank);
                return Ranking.Count;
            }
            for (int i = 0; i < Ranking.Count; i++)
            {
               //if level is lower
               if (Ranking[i].level < NewRank.level)
               {
                    Ranking.Insert(i, NewRank);
                    return i+1;
                }
                //if exp is lower but level = same
                if ((Ranking[i].level == NewRank.level) && (Ranking[i].Experience < NewRank.Experience))
                {
                   Ranking.Insert(i, NewRank);
                   return i+1;
                }
            }
            if (Ranking.Count < RankCount)
            {
                Ranking.Add(NewRank);
                return Ranking.Count;
            }
            return 0;
        }

        public bool TryAddRank(List<Rank_Character_Info> Ranking, CharacterInfo info, byte type)
        {
            Rank_Character_Info NewRank = new Rank_Character_Info() { Name = info.Name, Class = info.Class, Experience = info.Experience, level = info.Level, PlayerId = info.Index, info = info };
            int NewRankIndex = InsertRank(Ranking, NewRank);
            if (NewRankIndex == 0) return false;
            for (int i = NewRankIndex; i < Ranking.Count; i++ )
            {
                SetNewRank(Ranking[i], i + 1, type);
            }
            info.Rank[type] = NewRankIndex;
            return true;
        }

        public int FindRank(List<Rank_Character_Info> Ranking, CharacterInfo info, byte type)
        {
            int startindex = info.Rank[type];
            if (startindex > 0) //if there's a previously known rank then the user can only have gone down in the ranking (or stayed the same)
            {
                for (int i = startindex-1; i < Ranking.Count; i++)
                {
                    if (Ranking[i].Name == info.Name)
                        return i;
                }
                info.Rank[type] = 0;//set the rank to 0 to tell future searches it's not there anymore
            }
            else //if there's no previously known ranking then technicaly it shouldnt be listed, but check anyway?
            {
                //currently not used so not coded it < if there's a reason to, easy to add :p
            }
            return -1;//index can be 0
        }

        public void SetNewRank(Rank_Character_Info Rank, int Index, byte type)
        {
            CharacterInfo Player = Rank.info as CharacterInfo;
            if (Player == null) return;
            Player.Rank[type] = Index;
        }

        public void RemoveRank(CharacterInfo info)
        {
            List<Rank_Character_Info> Ranking;
            int Rankindex = -1;
            //first check overall top           
            if (info.Level >= RankBottomLevel[0])
            {
                Ranking = RankTop;
                Rankindex = FindRank(Ranking, info, 0);
                if (Rankindex >= 0)
                {
                    Ranking.RemoveAt(Rankindex);
                    for (int i = Rankindex; i < Ranking.Count(); i++)
                    {
                        SetNewRank(Ranking[i], i, 0);
                    }
                }
            }
            if (!RankBottomLevel.Contains((byte)info.Class + 1))
                return;
            //next class based top
            if (info.Level <= RankBottomLevel[(byte)info.Class + 1])
                return;

            Ranking = RankTop;
            Rankindex = FindRank(Ranking, info, 1);
            if (Rankindex >= 0)
            {
                Ranking.RemoveAt(Rankindex);
                for (int i = Rankindex; i < Ranking.Count(); i++)
                {
                    SetNewRank(Ranking[i], i, 1);
                }
            }
        }

        public bool UpdateRank(List<Rank_Character_Info> Ranking, CharacterInfo info, byte type)
        {
            int CurrentRank = FindRank(Ranking, info, type);
            if (CurrentRank == -1) return false;//not in ranking list atm
            
            int NewRank = CurrentRank;
            //next find our updated rank
            for (int i = CurrentRank-1; i >= 0; i-- )
            {
                if ((Ranking[i].level > info.Level) || ((Ranking[i].level == info.Level) && (Ranking[i].Experience > info.Experience))) break;
                    NewRank =i;
            }

            Ranking[CurrentRank].level = info.Level;
            Ranking[CurrentRank].Experience = info.Experience;

            if (NewRank < CurrentRank)
            {//if we gained any ranks
                Ranking.Insert(NewRank, Ranking[CurrentRank]);
                Ranking.RemoveAt(CurrentRank + 1);
                for (int i = NewRank + 1; i < Math.Min(Ranking.Count, CurrentRank +1); i++)
                {
                    SetNewRank(Ranking[i], i + 1, type);
                }
            }
            info.Rank[type] = NewRank+1;
            
            return true;
        }


        public void CheckRankUpdate(CharacterInfo info)
        {
            List<Rank_Character_Info> Ranking;
            Rank_Character_Info NewRank;
            
            //first check overall top           
            if (info.Level >= RankBottomLevel[0])
            {
                Ranking = RankTop;
                if (!UpdateRank(Ranking, info,0))
                {
                    if (TryAddRank(Ranking, info, 0))
                    {
                        if (Ranking.Count > RankCount)
                        {
                            SetNewRank(Ranking[RankCount], 0, 0);
                            Ranking.RemoveAt(RankCount);

                        }
                    }
                }
                if (Ranking.Count >= RankCount)
                { 
                    NewRank = Ranking[Ranking.Count -1];
                    if (NewRank != null)
                        RankBottomLevel[0] = NewRank.level;
                }
            }
            //now check class top
            if (info.Level >= RankBottomLevel[(byte)info.Class + 1])
            {
                Ranking = RankClass[(byte)info.Class];
                if (!UpdateRank(Ranking, info,1))
                {
                    if (TryAddRank(Ranking, info, 1))
                    {
                        if (Ranking.Count > RankCount)
                        {
                            SetNewRank(Ranking[RankCount], 0, 1);
                            Ranking.RemoveAt(RankCount);
                        }
                    }
                }
                if (Ranking.Count >= RankCount)
                {
                    NewRank = Ranking[Ranking.Count -1];
                    if (NewRank != null)
                        RankBottomLevel[(byte)info.Class + 1] = NewRank.level;
                }
            }
        }
    }
}
