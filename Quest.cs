
namespace Glyph.Entities
{
    public enum QuestState
    {
        Finished,
        Unfinished
    }

    public class Quest
    {
        public QuestState questState { get; set; }
        public string name;
        public string description; 

        public Quest(string name, string description, QuestState questState = QuestState.Unfinished)
        {
            this.name = name;
            this.description = description;
            this.questState = questState;
        }

    }
}
