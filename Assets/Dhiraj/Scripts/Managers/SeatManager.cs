using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Dhiraj
{
    public class SeatManager : MonoBehaviour
    {
        public List<Seat> ListOfSeats = new List<Seat>();

        private void Awake()
        {
            foreach (Transform t in transform)
            {
                ListOfSeats.Add(t.GetComponent<Seat>());
            }
        }


        public Seat GetEmptySeat()
        {
            var emptySeats = ListOfSeats.Where(seat => !seat.isOccupied).ToList();

            if (emptySeats.Count == 0)
                return null;

            int randomIndex = Random.Range(0, emptySeats.Count);
            return emptySeats[randomIndex];
        }
        public Seat ReserveEmptySeat(AManager requester)
        {
            var seat = ListOfSeats.FirstOrDefault(s => !s.isOccupied);
            if (seat != null)
            {
                seat.isOccupied = true;
                seat.occupiedBy = requester;
            }
            return seat;
        }
        public Seat CheckForEmptySeat()
        {
            if (ListOfSeats.Count > 0)
            {
                foreach (var item in ListOfSeats)
                {
                    if (!item.isOccupied)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public AManager GetOccupiedSeat(AManager thisManager)
        {
            AManager otherManager;
            if (ListOfSeats.Count > 0)
            {
                foreach (var item in ListOfSeats)
                {
                    if (item.isOccupied)
                    {
                        otherManager = item.occupiedBy;
                        otherManager.enemyTargets.Add(thisManager);
                        otherManager.CombatStart();
                        thisManager.CombatStart();
                        return otherManager;
                    }
                }
            }
            return null;
        }
    }

}
