using System.Collections.Generic;
using System.Linq;
using System; 

namespace Glyph.Entities
{
    public struct PriorityQuest  
    {
        public Quest quest;
        public int priority;

        public PriorityQuest(Quest quest, int priority)
        {
            this.quest = quest;
            this.priority = priority;
        }
    }

    public class HandleQuests 
    {
        List<PriorityQuest> questQueue = new List<PriorityQuest>();

        public HandleQuests(List<PriorityQuest> priorityQuests)
        {
            InsertToQueue(priorityQuests);
        }

        public void InsertToQueue(List<PriorityQuest> priorityQuests)
        {
            questQueue.AddRange(priorityQuests);
            questQueue = questQueue.OrderBy(x => x.priority).ToList();
        }

        public void Dequeue()
        {
            // Handles only small queue's efficiently 
            for (int i = 0; questQueue.Count > 0 && i < questQueue.Count; i++)
            {
                if ( questQueue[i].quest.questState == QuestState.Finished)
                {
                    questQueue.RemoveAt(i);
                    i -= 1; 
                }
            }
        }
    }
}
