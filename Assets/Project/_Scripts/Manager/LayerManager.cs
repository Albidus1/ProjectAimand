using UnityEngine;

public static class LayerManager
{
    private static int playerLayer = 6;
    private static int enemiesLayer = 25;
    private static int projectilesLayer = 17;
    private static int platformsLayer = 7;
    private static int movingPlatformsLayer = 8;
    private static int onewayPlatformsLayer = 9;
    private static int movingObjectsLayer = 11;

    public static int playerLayerMask = 1 << playerLayer;
    public static int enemiesLayerMask = 1 << enemiesLayer;
    public static int projectilesLayerMask = 1 << projectilesLayer;
    public static int platformsLayerMask = 1 << platformsLayer;
    public static int movingPlatformsLayerMask = 1 << movingPlatformsLayer;
    public static int onewayPlatformsLayerMask = 1 << onewayPlatformsLayer;
    public static int movingObjectsLayerMask = 1 << movingObjectsLayer;

    public static int obstacleLayerMask = LayerManager.platformsLayerMask |
                                           LayerManager.movingPlatformsLayerMask |
                                           LayerManager.onewayPlatformsLayerMask;
}
