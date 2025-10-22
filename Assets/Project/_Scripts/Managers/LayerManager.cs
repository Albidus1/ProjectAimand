using UnityEngine;

public static class LayerManager
{
    private static int playerLayer = 31;
    private static int projectilesLayer = 20;
    private static int platformsLayer = 11;
    private static int movingPlatformsLayer = 12;
    private static int onewayPlatformsLayer = 13;
    private static int killOnTouchPlatformsLayer = 14;
    //private static int movingObjectsLayer = 11;

    public static int playerLayerMask = 1 << playerLayer;
    public static int projectilesLayerMask = 1 << projectilesLayer;
    public static int platformsLayerMask = 1 << platformsLayer;
    public static int movingPlatformsLayerMask = 1 << movingPlatformsLayer;
    public static int onewayPlatformsLayerMask = 1 << onewayPlatformsLayer;
    public static int killOnTouchPlatformsLayerMask = 1 << killOnTouchPlatformsLayer;
    //public static int movingObjectsLayerMask = 1 << movingObjectsLayer;

    public static int obstacleLayerMask = LayerManager.platformsLayerMask |
                                           LayerManager.movingPlatformsLayerMask |
                                           LayerManager.onewayPlatformsLayerMask;
}
