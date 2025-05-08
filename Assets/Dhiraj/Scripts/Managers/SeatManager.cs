using System.Collections.Generic;
using System.IO;
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
            if (ListOfSeats.Count > 0)
            {
                foreach (var item in ListOfSeats)
                {
                    if (!item.isOccupied) return item;
                }
            }
            return null;
        }
    }

}
