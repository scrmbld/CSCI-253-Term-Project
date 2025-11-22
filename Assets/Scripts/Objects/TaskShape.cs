using UnityEngine;
using UnityEngine.Events;

namespace TaskShape
{
    public enum ShapeColor
    {
        Red,
        Green,
        Blue,
        Purple
    }

    public enum ShapeType
    {
        Cube,
        Sphere,
        Cylinder
    }

    /// <summary>
    /// The main purpose of this interface is to provide the information needed to tell if
    /// our object has reached its goal state.
    /// </summary>
    public interface Shape
    {
        static float translationThreshold = 0.3f;
        static float rotationThreshold = 10.0f;
        /// <summary>
        /// Returns true if this shape and the one passed in have symmetry equivalent transforms, within a threshold. 
        /// </summary>
        /// <param name="other">The shape to compare against.</param>
        /// <returns>Whether the two shapes have equivalent transforms.</returns>
        bool Equivalent(Shape other);

        /// <summary>
        /// Returns the type of shape that the current shape is. This is needed for the
        /// equivalent function -- while two spheres would be equivalent given the same 
        /// position and any rotation, a sphere and a cube cannot be equivalent under
        /// the same conditions.
        /// </summary>
        /// <returns></returns>
        ShapeType Type();

        ShapeColor Color(); 

        /// <summary>
        /// Returns the transform of the shape. This is needed for the Equivalent function.
        /// </summary>
        /// <returns></returns>
        Transform ShapeTransform();
    }

    public class Cube : Shape
    {
        // the object that the Cube represents
        public GameObject obj;
        public ShapeColor color;

        public Cube(GameObject newObj, ShapeColor newColor)
        {
            obj = newObj;
            color = newColor;
        }

        public bool Equivalent(Shape other)
        {
            if (other.Type() != ShapeType.Cube || other.Color() != color)
            {
                return false;
            }

            float transDelta = (obj.transform.position - other.ShapeTransform().position).magnitude;

            float rotDelta = Angles.SymmetryDelta(
                    obj.transform.rotation,
                    other.ShapeTransform().rotation,
                    new Vector3(4, 4, 4)
                );
            return rotDelta < Shape.rotationThreshold && transDelta < Shape.translationThreshold;
        }

        public ShapeType Type()
        {
            return ShapeType.Cube;
        }

        public ShapeColor Color()
        {
            return color;
        }

        public Transform ShapeTransform()
        {
            return obj.transform;
        }
    }

    public class Sphere : Shape
    {
        public GameObject obj;
        public ShapeColor color;

        public Sphere(GameObject newObj, ShapeColor newColor)
        {
            obj = newObj;
            color = newColor;
        }

        public bool Equivalent(Shape other)
        {
            if (other.Type() != ShapeType.Sphere || other.Color() != color)
            {
                return false;
            }

            float transDelta = Vector3.Distance(obj.transform.position, other.ShapeTransform().position);

            // no need to check rotation because we are 100% spherically symmetrical
            return transDelta < Shape.translationThreshold;
        }

        public ShapeType Type()
        {
            return ShapeType.Sphere;
        }

        public ShapeColor Color()
        {
            return color;
        }

        public Transform ShapeTransform()
        {
            return obj.transform;
        }
    }

    public class Cylinder : Shape 
    {
        public GameObject obj;
        public ShapeColor color;

        public Cylinder(GameObject newObj, ShapeColor newColor)
        {
            obj = newObj;
            color = newColor;
        }

        public bool Equivalent(Shape other)
        {
            if (other.Type() != ShapeType.Cylinder || other.Color() != color)
            {
                return false;
            }

            float transDelta = Vector3.Distance(obj.transform.position, other.ShapeTransform().position);
            float rotDelta = Angles.SymmetryDelta(
                    obj.transform.rotation,
                    other.ShapeTransform().rotation,
                    new Vector3(2, 360, 2)
                );

            return rotDelta < Shape.rotationThreshold && transDelta < Shape.translationThreshold;
        }

        public ShapeType Type()
        {
            return ShapeType.Cylinder;
        }

        public ShapeColor Color()
        {
            return color;
        }

        public Transform ShapeTransform()
        {
            return obj.transform;
        }
    }

    public class Angles
    {
        /// <summary>
        /// Gets the Euler angles difference between a and b, accounting for the symmetries
        /// defined by the <c>symmetries</c> argument.
        /// </summary>
        /// <param name="a">Euler angle</param>
        /// <param name="b">Euler angle</param>
        /// <param name="symmetries">
        /// Defines the angle distance between rotational symmetries on each axis. Assumes
        /// that symmetries on the same axis are equidistant. Also assumes that symmetries
        /// can be obtained by rotating directly around the Euler angle axes.
        /// </param>
        /// <returns>
        /// A Vector3 containing the Euler angle distance between a and b, accounting for rotational symmetries.
        /// </returns>
        static public float SymmetryDelta(Quaternion a, Quaternion b, Vector3 symmetries)
        {
            Transform t = new GameObject("angles").transform;
            t.position = Vector3.zero;
            t.rotation = a;
            float minimumDelta = 360.0f;

            // iterate over all possible symmetries
            for (int j = 0; j < symmetries.x; j++)
            {
                t.RotateAround(t.position, t.right, 360.0f / symmetries.x);
                for (int k = 0; k < symmetries.y; k++)
                {
                    t.RotateAround(t.position, t.up, 360.0f / symmetries.y);
                    for (int l = 0; l < symmetries.z; l++)
                    {
                        t.RotateAround(t.position, t.forward, 360.0f / symmetries.z);
                        if (Quaternion.Angle(t.rotation, b) < minimumDelta)
                        {
                            minimumDelta = Quaternion.Angle(t.rotation, b);
                        }
                    }
                }
            }

            return minimumDelta;
        }
    }

    /// <summary>
    /// An event used to indicate that an object has been successfully placed in its goal
    /// location. Contents are the item object and the goal object.
    /// </summary>
    public class GoalEvent : UnityEvent<GameObject, GameObject> { }
}