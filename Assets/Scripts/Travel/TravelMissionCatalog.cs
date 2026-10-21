/// <summary>
/// 旅行任务静态目录（中国城市 / 地标主题命名）。无需 ScriptableObject 资产即可验收。
/// </summary>
public static class TravelMissionCatalog
{
    public struct Entry
    {
        public string id;
        public string title;
        public string hint;
        public float x;
        public float z;

        public Entry(string id, string title, string hint, float x, float z)
        {
            this.id = id;
            this.title = title;
            this.hint = hint;
            this.x = x;
            this.z = z;
        }
    }

    public static readonly Entry[] Missions =
    {
        new Entry("westlake", "西湖断桥", "步行前往黄色光圈：杭州西湖断桥", -10f, 14f),
        new Entry("greatwall", "长城烽火台", "步行前往黄色光圈：八达岭长城烽火台", 12f, 18f),
        new Entry("huangshan", "黄山迎客松", "步行前往黄色光圈：黄山迎客松观景台", 0f, 32f)
    };
}
