using UnityEngine;

public class PlayerMovementBoundsController
{
    private const float MovementBoundsEpsilon = 0.001f;

    private Collider2D movementBoundsSource;
    private Bounds movementBounds;
    private bool hasMovementBounds;

    public void SetMovementBounds(Collider2D boundsCollider)
    {
        if (boundsCollider == null)
        {
            hasMovementBounds = false;
            movementBoundsSource = null;
            return;
        }

        movementBoundsSource = boundsCollider;
        movementBounds = boundsCollider.bounds;
        hasMovementBounds = true;
    }

    public void ClearMovementBounds(Collider2D boundsCollider)
    {
        if (movementBoundsSource != boundsCollider)
        {
            return;
        }

        hasMovementBounds = false;
        movementBoundsSource = null;
    }

    public void ClampControlledBodyToBounds(Rigidbody2D controlledBody, Collider2D activeCollider)
    {
        if (!hasMovementBounds || controlledBody == null)
        {
            return;
        }

        Bounds activeBounds = movementBoundsSource != null ? movementBoundsSource.bounds : movementBounds;
        Vector2 currentPosition = controlledBody.position;
        Vector2 clampedPosition = GetClampedControlledPosition(desiredPosition: currentPosition, activeBounds, activeCollider);

        if ((clampedPosition - currentPosition).sqrMagnitude <= 0.000001f)
        {
            return;
        }

        controlledBody.position = clampedPosition;

        if (!Mathf.Approximately(clampedPosition.x, currentPosition.x))
        {
            controlledBody.linearVelocityX = 0f;
        }

        if (!Mathf.Approximately(clampedPosition.y, currentPosition.y))
        {
            controlledBody.linearVelocityY = 0f;
        }
    }

    public Vector2 FilterVelocityAgainstBounds(Rigidbody2D controlledBody, Collider2D activeCollider, Vector2 desiredVelocity)
    {
        if (!hasMovementBounds || controlledBody == null)
        {
            return desiredVelocity;
        }

        Bounds activeBounds = movementBoundsSource != null ? movementBoundsSource.bounds : movementBounds;
        Vector2 currentPosition = controlledBody.position;
        float deltaTime = Mathf.Max(Time.fixedDeltaTime, 0.0001f);
        Vector2 desiredPosition = currentPosition + desiredVelocity * deltaTime;
        Vector2 clampedPosition = GetClampedControlledPosition(desiredPosition, activeBounds, activeCollider);
        return (clampedPosition - currentPosition) / deltaTime;
    }

    private Vector2 GetClampedControlledPosition(Vector2 desiredPosition, Bounds activeBounds, Collider2D activeCollider)
    {
        if (movementBoundsSource is EdgeCollider2D edgeCollider
            && TryGetClosedEdgeWorldPoints(edgeCollider, out Vector2[] worldPoints, out int pointCount))
        {
            return ClampPositionToClosedEdge(desiredPosition, worldPoints, pointCount);
        }

        GetControlledPositionLimits(activeBounds, activeCollider, out float minX, out float maxX, out float minY, out float maxY);

        Vector2 colliderCenterOffset = Vector2.zero;

        if (activeCollider != null)
        {
            colliderCenterOffset = (Vector2)(activeCollider.bounds.center - activeCollider.transform.position);
        }

        if (minX > maxX)
        {
            desiredPosition.x = activeBounds.center.x - colliderCenterOffset.x;
        }
        else
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        }

        if (minY > maxY)
        {
            desiredPosition.y = activeBounds.center.y - colliderCenterOffset.y;
        }
        else
        {
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        return desiredPosition;
    }

    private bool TryGetClosedEdgeWorldPoints(EdgeCollider2D edgeCollider, out Vector2[] worldPoints, out int pointCount)
    {
        worldPoints = null;
        pointCount = 0;

        if (edgeCollider == null || edgeCollider.pointCount < 3)
        {
            return false;
        }

        Vector2[] localPoints = edgeCollider.points;
        worldPoints = new Vector2[localPoints.Length];

        for (int i = 0; i < localPoints.Length; i++)
        {
            worldPoints[i] = edgeCollider.transform.TransformPoint(localPoints[i] + edgeCollider.offset);
        }

        pointCount = worldPoints.Length;

        if ((worldPoints[0] - worldPoints[pointCount - 1]).sqrMagnitude <= MovementBoundsEpsilon * MovementBoundsEpsilon)
        {
            pointCount--;
        }

        return pointCount >= 3;
    }

    private Vector2 ClampPositionToClosedEdge(Vector2 desiredPosition, Vector2[] worldPoints, int pointCount)
    {
        if (IsPointInsideClosedEdge(desiredPosition, worldPoints, pointCount)
            || IsPointOnClosedEdge(desiredPosition, worldPoints, pointCount))
        {
            return desiredPosition;
        }

        Vector2 closestPoint = GetClosestPointOnClosedEdge(desiredPosition, worldPoints, pointCount);
        Vector2 centroid = GetClosedEdgeCentroid(worldPoints, pointCount);
        Vector2 inwardDirection = centroid - closestPoint;

        if (inwardDirection.sqrMagnitude > 0.000001f)
        {
            closestPoint += inwardDirection.normalized * MovementBoundsEpsilon;
        }

        return closestPoint;
    }

    private bool IsPointInsideClosedEdge(Vector2 point, Vector2[] worldPoints, int pointCount)
    {
        bool isInside = false;

        for (int i = 0, j = pointCount - 1; i < pointCount; j = i++)
        {
            Vector2 a = worldPoints[i];
            Vector2 b = worldPoints[j];
            bool intersects = ((a.y > point.y) != (b.y > point.y))
                && (point.x < (b.x - a.x) * (point.y - a.y) / ((b.y - a.y) + Mathf.Epsilon) + a.x);

            if (intersects)
            {
                isInside = !isInside;
            }
        }

        return isInside;
    }

    private bool IsPointOnClosedEdge(Vector2 point, Vector2[] worldPoints, int pointCount)
    {
        for (int i = 0; i < pointCount; i++)
        {
            Vector2 segmentStart = worldPoints[i];
            Vector2 segmentEnd = worldPoints[(i + 1) % pointCount];
            Vector2 closestPoint = GetClosestPointOnSegment(point, segmentStart, segmentEnd);

            if ((closestPoint - point).sqrMagnitude <= MovementBoundsEpsilon * MovementBoundsEpsilon)
            {
                return true;
            }
        }

        return false;
    }

    private Vector2 GetClosestPointOnClosedEdge(Vector2 point, Vector2[] worldPoints, int pointCount)
    {
        Vector2 closestPoint = worldPoints[0];
        float closestDistanceSqr = float.MaxValue;

        for (int i = 0; i < pointCount; i++)
        {
            Vector2 segmentStart = worldPoints[i];
            Vector2 segmentEnd = worldPoints[(i + 1) % pointCount];
            Vector2 candidatePoint = GetClosestPointOnSegment(point, segmentStart, segmentEnd);
            float candidateDistanceSqr = (candidatePoint - point).sqrMagnitude;

            if (candidateDistanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = candidateDistanceSqr;
                closestPoint = candidatePoint;
            }
        }

        return closestPoint;
    }

    private Vector2 GetClosestPointOnSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
    {
        Vector2 segment = segmentEnd - segmentStart;
        float segmentLengthSqr = segment.sqrMagnitude;

        if (segmentLengthSqr <= Mathf.Epsilon)
        {
            return segmentStart;
        }

        float t = Mathf.Clamp01(Vector2.Dot(point - segmentStart, segment) / segmentLengthSqr);
        return segmentStart + segment * t;
    }

    private Vector2 GetClosedEdgeCentroid(Vector2[] worldPoints, int pointCount)
    {
        Vector2 centroid = Vector2.zero;
        float signedArea = 0f;

        for (int i = 0; i < pointCount; i++)
        {
            Vector2 current = worldPoints[i];
            Vector2 next = worldPoints[(i + 1) % pointCount];
            float cross = current.x * next.y - next.x * current.y;

            signedArea += cross;
            centroid += (current + next) * cross;
        }

        if (Mathf.Abs(signedArea) <= Mathf.Epsilon)
        {
            for (int i = 0; i < pointCount; i++)
            {
                centroid += worldPoints[i];
            }

            return centroid / pointCount;
        }

        return centroid / (3f * signedArea);
    }

    private void GetControlledPositionLimits(Bounds activeBounds, Collider2D activeCollider, out float minX, out float maxX, out float minY, out float maxY)
    {
        Vector2 colliderExtents = Vector2.zero;
        Vector2 colliderCenterOffset = Vector2.zero;

        if (activeCollider != null)
        {
            Bounds colliderBounds = activeCollider.bounds;
            colliderExtents = colliderBounds.extents;
            colliderCenterOffset = (Vector2)(colliderBounds.center - activeCollider.transform.position);
        }

        minX = activeBounds.min.x + colliderExtents.x - colliderCenterOffset.x;
        maxX = activeBounds.max.x - colliderExtents.x - colliderCenterOffset.x;
        minY = activeBounds.min.y + colliderExtents.y - colliderCenterOffset.y;
        maxY = activeBounds.max.y - colliderExtents.y - colliderCenterOffset.y;
    }
}
