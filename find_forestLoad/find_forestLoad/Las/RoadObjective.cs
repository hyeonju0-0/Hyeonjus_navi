namespace find_forestLoad.Las; // RoadStandard.cs의 namespace와 다르면 수정

public enum RoadObjective
{
    Safety,          // 경사와 절토·성토를 낮게
    Cost,            // 길이와 절토·성토량 추정치를 낮게
    Distance,        // 경로 길이를 짧게
    ConstructionTime // 길이와 공사 난이도 추정치를 낮게
}