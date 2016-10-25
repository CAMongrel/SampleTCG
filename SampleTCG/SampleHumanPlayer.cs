using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleTCG
{
   class SampleHumanPlayer : SamplePlayer
   {
      public SampleHumanPlayer()
         : base(30)
      {
         IsLocallyControlled = true;
      }
   }
}
