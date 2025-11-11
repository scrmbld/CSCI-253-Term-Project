using UnityEngine;

namespace TaskShape
{
    public enum ShapeColor
    {
        Gray,
        Green,
        Orange,
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
        static float translationThreshold = 1.0f;
        static float rotationThreshold = 20.0f;
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

            Vector3 rotDelta = Angles.SymmetryDelta(
                    obj.transform.rotation.eulerAngles,
                    other.ShapeTransform().rotation.eulerAngles,
                    new Vector3(90.0f, 90.0f, 90.0f)
                );
            Debug.Log(rotDelta);
            return rotDelta.x < Shape.rotationThreshold
                   && rotDelta.y < Shape.rotationThreshold
                   && rotDelta.z < Shape.rotationThreshold
                   && transDelta < Shape.translationThreshold;
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
            Vector3 rotDelta = Angles.SymmetryDelta(
                    obj.transform.rotation.eulerAngles,
                    other.ShapeTransform().rotation.eulerAngles,
                    new Vector3(180.0f, 0.001f, 180.0f)
                );

            return rotDelta.x < Shape.rotationThreshold
                   && rotDelta.y < Shape.rotationThreshold
                   && rotDelta.z < Shape.rotationThreshold
                   && transDelta < Shape.translationThreshold;
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

    class Angles
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
        static public Vector3 SymmetryDelta(Vector3 a, Vector3 b, Vector3 symmetries)
        {
            // modulus the things
            Vector3 a_symmetric = new Vector3(a.x % symmetries.x, a.y % symmetries.y, a.z % symmetries.z);
            Vector3 b_symmetric = new Vector3(b.x % symmetries.x, b.y % symmetries.y, b.z % symmetries.z);

            // account for the fact that we might be closer to the symmetry above us than the one below us
            float inverted;
            for (int i = 0; i < 3; i++)
            {
                inverted = Mathf.Abs(a_symmetric[i] - symmetries[i]);
                a_symmetric[i] = inverted < a_symmetric[i] ? inverted : a_symmetric[i];
            }
            for (int i = 0; i < 3; i++)
            {
                inverted = Mathf.Abs(b_symmetric[i] - symmetries[i]);
                b_symmetric[i] = inverted < b_symmetric[i] ? inverted : b_symmetric[i];
            }

            // return the absolute value of the difference
            Vector3 delta = a_symmetric - b_symmetric;
            return new Vector3(Mathf.Abs(delta.x), Mathf.Abs(delta.y), Mathf.Abs(delta.z));
        }
    }
}