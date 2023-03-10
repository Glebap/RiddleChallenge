using UnityEngine;


public class BoardView : MonoBehaviour
{
    [SerializeField] private GameObject _boundsOfView;
    [SerializeField] private Transform _boardUnitsTransform;
    [SerializeField] private BoardUnit _boardUnitPrefab;
    [SerializeField, Min(10)] private float _cameraDistance;
    [SerializeField, Min(1)] private float _marginScale;

    private Camera _camera;
    private int _height;
    private int _width;

    public float CameraDistance => _cameraDistance;

    
    public BoardUnit[,] Initialize(Vector2Int size)
    {
        _camera = Camera.main;
        _height = size.x;
        _width = size.y;
        
        var boardUnit = createBoardUnits();
        SetUpBoundsOfView();
        SetUpCamera();

        return boardUnit;
    }

    private void SetUpBoundsOfView()
    {
        var boundsOfViewPosition = new Vector3((_width - 1.0f) / 2.0f, 0.0f, (_height - 1.0f) / 2.0f + _height * 0.1f);
        _boundsOfView.transform.localPosition = boundsOfViewPosition;
        _boundsOfView.transform.localScale = new Vector3(_width, 0.5f, 2.0f + _height * 1.6f);

    }
    
    private void SetUpCamera()
    {
        var bounds = _boundsOfView.GetComponent<Renderer>().bounds;
        var cameraTransform = _camera.transform;
        float fieldOfView;
        
        if(bounds.size.x / bounds.size.z > _camera.aspect)
        {
            var centerToTopXDist = Mathf.Abs(bounds.center.x - bounds.max.x);
            var horizontalFieldOfView = 2.0f * Mathf.Atan(centerToTopXDist / _cameraDistance) * 180 / Mathf.PI;
            fieldOfView = Camera.HorizontalToVerticalFieldOfView(horizontalFieldOfView, _camera.aspect);
        }
        else
        {
            var centerToTopZDist = Mathf.Abs(bounds.center.z - bounds.max.z);
            fieldOfView = 2.0f * Mathf.Atan( centerToTopZDist / _cameraDistance) * 180/Mathf.PI;
        }
        
        _camera.fieldOfView = fieldOfView * _marginScale;
        cameraTransform.position = new Vector3(bounds.center.x, _cameraDistance, bounds.center.z);
        cameraTransform.LookAt(bounds.center);

    }

    private BoardUnit[,] createBoardUnits()
    {
        const float boardUnitPositionY = 0.01f;
        var boardUnits = new BoardUnit[_height,_width];
        
        for (var x = 0; x < _width; x++) {
            for (var z = 0; z < _height; z++)
            {
                var boardUnit = Instantiate(_boardUnitPrefab, _boardUnitsTransform);
                boardUnit.transform.localPosition = new Vector3(x, boardUnitPositionY, z);
                var isAlternative = (x & 1) == 0;
                if ((z & 1) == 0) isAlternative = !isAlternative;
                boardUnit.Initialize(new Vector2Int(x, z), isAlternative);
                boardUnits[x, z] = boardUnit;
            }
        }
        
        return boardUnits;
    }
}
