/** Vector Math class. */
export default class Vector {
    constructor(a, b, c) {
        var _a, _b, _c, _d, _e, _f;
        if (Array.isArray(a)) {
            this.x = (_a = a[0]) !== null && _a !== void 0 ? _a : 0;
            this.y = (_b = a[1]) !== null && _b !== void 0 ? _b : 0;
            this.z = (_c = a[2]) !== null && _c !== void 0 ? _c : 0;
            return;
        }
        if (!!a && typeof a === "object") {
            this.x = (_d = a.x) !== null && _d !== void 0 ? _d : 0;
            this.y = (_e = a.y) !== null && _e !== void 0 ? _e : 0;
            this.z = (_f = a.z) !== null && _f !== void 0 ? _f : 0;
            return;
        }
        this.x = a !== null && a !== void 0 ? a : 0;
        this.y = b !== null && b !== void 0 ? b : 0;
        this.z = c !== null && c !== void 0 ? c : 0;
    }
    // Methods //
    /**
     * Returns the negative of this vector.
     */
    negative() {
        return new Vector(-this.x, -this.y, -this.z);
    }
    /**
     * Add a vector or number to this vector.
     * @param {Vector | number} a: Vector or number to add
     * @returns {Vector} New vector
     */
    add(v) {
        if (v instanceof Vector)
            return new Vector(this.x + v.x, this.y + v.y, this.z + v.z);
        else
            return new Vector(this.x + v, this.y + v, this.z + v);
    }
    /**
     * Substracts a vector or number from this vector.
     * @param {Vector | number} a: Vector or number to subtract
     * @returns {Vector} New vector
     */
    subtract(v) {
        if (v instanceof Vector)
            return new Vector(this.x - v.x, this.y - v.y, this.z - v.z);
        else
            return new Vector(this.x - v, this.y - v, this.z - v);
    }
    /**
     * Multiplies a vector or a number to a vector.
     * @param {Vector | number} a: Vector or number to multiply
     * @param {Vector} b: Vector to multiply
     */
    multiply(v) {
        if (v instanceof Vector)
            return new Vector(this.x * v.x, this.y * v.y, this.z * v.z);
        else
            return new Vector(this.x * v, this.y * v, this.z * v);
    }
    /**
     * Divide this vector by a vector or a number.
     * @param {Vector | number} a: Vector or number to divide
     * @returns {Vector} New vector
     */
    divide(v) {
        if (v instanceof Vector)
            return new Vector(this.x / v.x, this.y / v.y, this.z / v.z);
        else
            return new Vector(this.x / v, this.y / v, this.z / v);
    }
    /**
     * Check if the given vector is equal to this vector.
     * @param {Vector} v: Vector to compare
     * @returns {boolean} True if equal
     */
    equals(v) {
        return this.x == v.x && this.y == v.y && this.z == v.z;
    }
    /**
     * Returns the dot product of this vector and another vector.
     * @param {Vector} v: Vector to dot
     * @returns {number} Dot product
     */
    dot(v) {
        return this.x * v.x + this.y * v.y + this.z * v.z;
    }
    /**
     * Cross product of two vectors.
     * @param {Vector} a: Vector to cross
     * @param {Vector} b: Vector to cross
     */
    cross(v) {
        return new Vector(this.y * v.z - this.z * v.y, this.z * v.x - this.x * v.z, this.x * v.y - this.y * v.x);
    }
    /**
     * Get the length of the Vector
     * @returns {number} Length
     */
    length() {
        return Math.sqrt(this.dot(this));
    }
    /**
     * Find the distance between this and another vector.
     * @param {Vector} v: Vector to find distance to
     * @param {2 | 3} d: 2D or 3D distance
     * @returns {number} Distance
     */
    distance(v, d = 3) {
        //2D distance
        if (d === 2)
            return Math.sqrt(Math.pow(this.x - v.x, 2) + Math.pow(this.y - v.y, 2));
        //3D distance
        else
            return Math.sqrt(Math.pow(this.x - v.x, 2) + Math.pow(this.y - v.y, 2) + Math.pow(this.z - v.z, 2));
    }
    /**
     * Lerp between this vector and another vector.
     * @param {Vector} v: Vector to lerp to
     * @param {number} fraction: Fraction to lerp
     * @returns {Vector}
     */
    lerp(v, fraction) {
        return v.subtract(this).multiply(fraction).add(this);
    }
    /**
     * Returns the unit vector of this vector.
     * @returns {Vector} Unit vector
     */
    unit() {
        return this.divide(this.length());
    }
    min() {
        return Math.min(Math.min(this.x, this.y), this.z);
    }
    max() {
        return Math.max(Math.max(this.x, this.y), this.z);
    }
    /**
     * To Angles
     * @returns {{ theta: number, phi: number }}
     */
    toAngles() {
        return {
            theta: Math.atan2(this.z, this.x),
            phi: Math.asin(this.y / this.length()),
        };
    }
    angleTo(a) {
        return Math.acos(this.dot(a) / (this.length() * a.length()));
    }
    /**
     * Array representation of the vector.
     * @param {number} n: Array length
     * @returns {number[]} Array
     * @example
     * new Vector(1, 2, 3).toArray(); // [1, 2, 3]
     */
    toArray(n) {
        return [this.x, this.y, this.z].slice(0, n || 3);
    }
    /**
     * Clone the vector.
     * @returns {Vector} New vector
     */
    clone() {
        return new Vector(this.x, this.y, this.z);
    }
    /**
     * Init this Vector with explicit values
     * @param {number} x: X value
     * @param {number} y: Y value
     * @param {number} z: Z value
     */
    init(x, y, z) {
        this.x = x;
        this.y = y;
        this.z = z;
        return this;
    }
    // static methods //
    static negative(a, b = new Vector()) {
        b.x = -a.x;
        b.y = -a.y;
        b.z = -a.z;
        return b;
    }
    static add(a, b, c = new Vector()) {
        if (b instanceof Vector) {
            c.x = a.x + b.x;
            c.y = a.y + b.y;
            c.z = a.z + b.z;
        }
        else {
            c.x = a.x + b;
            c.y = a.y + b;
            c.z = a.z + b;
        }
        return c;
    }
    static subtract(a, b, c = new Vector()) {
        if (b instanceof Vector) {
            c.x = a.x - b.x;
            c.y = a.y - b.y;
            c.z = a.z - b.z;
        }
        else {
            c.x = a.x - b;
            c.y = a.y - b;
            c.z = a.z - b;
        }
        return c;
    }
    static multiply(a, b, c = new Vector()) {
        if (b instanceof Vector) {
            c.x = a.x * b.x;
            c.y = a.y * b.y;
            c.z = a.z * b.z;
        }
        else {
            c.x = a.x * b;
            c.y = a.y * b;
            c.z = a.z * b;
        }
        return c;
    }
    static divide(a, b, c = new Vector()) {
        if (b instanceof Vector) {
            c.x = a.x / b.x;
            c.y = a.y / b.y;
            c.z = a.z / b.z;
        }
        else {
            c.x = a.x / b;
            c.y = a.y / b;
            c.z = a.z / b;
        }
        return c;
    }
    static cross(a, b, c = new Vector()) {
        c.x = a.y * b.z - a.z * b.y;
        c.y = a.z * b.x - a.x * b.z;
        c.z = a.x * b.y - a.y * b.x;
        return c;
    }
    static unit(a, b) {
        let length = a.length();
        b.x = a.x / length;
        b.y = a.y / length;
        b.z = a.z / length;
        return b;
    }
    /**
     * Create new vector from angles
     * @param {number} theta: Theta angle
     * @param {number} phi: Phi angle
     * @returns {Vector} New vector
     */
    static fromAngles(theta, phi) {
        return new Vector(Math.cos(theta) * Math.cos(phi), Math.sin(phi), Math.sin(theta) * Math.cos(phi));
    }
    static randomDirection() {
        return Vector.fromAngles(Math.random() * Math.PI * 2, Math.asin(Math.random() * 2 - 1));
    }
    static min(a, b) {
        return new Vector(Math.min(a.x, b.x), Math.min(a.y, b.y), Math.min(a.z, b.z));
    }
    static max(a, b) {
        return new Vector(Math.max(a.x, b.x), Math.max(a.y, b.y), Math.max(a.z, b.z));
    }
    /**
     * Lerp between two vectors
     * @param {Vector} a: Vector a
     * @param {Vector} b: Vector b
     * @param {number} fraction: Fraction
     */
    static lerp(a, b, fraction) {
        if (b instanceof Vector) {
            return b.subtract(a).multiply(fraction).add(a);
        }
        else {
            return ((b - a) * fraction + a);
        }
    }
    /**
     * Create a new vector from an Array
     * @param {number[]} array: Array
     * @returns {Vector} New vector
     */
    static fromArray(a) {
        if (Array.isArray(a)) {
            return new Vector(a[0], a[1], a[2]);
        }
        return new Vector(a.x, a.y, a.z);
    }
    /**
     * Angle between two vectors
     * @param {Vector} a: Vector a
     * @param {Vector} b: Vector b
     * @returns
     */
    static angleBetween(a, b) {
        return a.angleTo(b);
    }
    /**
     * Angle between two vertices
     * @param a
     * @param b
     * @param c
     */
    static angleBetweenVertices(a, b, c) {
        let ab = a.subtract(b);
        let bc = c.subtract(b);
    }
    static distance(a, b, d) {
        if (d === 2)
            return Math.sqrt(Math.pow(a.x - b.x, 2) + Math.pow(a.y - b.y, 2));
        else
            return Math.sqrt(Math.pow(a.x - b.x, 2) + Math.pow(a.y - b.y, 2) + Math.pow(a.z - b.z, 2));
    }
    static toDegrees(a) {
        return a * (180 / Math.PI);
    }
    static normalizeAngle(radians) {
        let TWO_PI = Math.PI * 2;
        let angle = radians % TWO_PI;
        angle = angle > Math.PI ? angle - TWO_PI : angle < -Math.PI ? TWO_PI + angle : angle;
        //returns normalized values to -1,1
        return angle / Math.PI;
    }
    static normalizeRadians(radians) {
        if (radians >= Math.PI / 2) {
            radians -= 2 * Math.PI;
        }
        if (radians <= -Math.PI / 2) {
            radians += 2 * Math.PI;
            radians = Math.PI - radians;
        }
        //returns normalized values to -1,1
        return radians / Math.PI;
    }
    static find2DAngle(cx, cy, ex, ey) {
        var dy = ey - cy;
        var dx = ex - cx;
        var theta = Math.atan2(dy, dx);
        return theta;
    }
    /**
     * Find 3D rotation between two vectors
     * @param {Vector} a: First vector
     * @param {Vector} b: Second vector
     * @param {boolean} normalize: Normalize the result
     */
    static findRotation(a, b, normalize = true) {
        if (normalize) {
            return new Vector(Vector.normalizeRadians(Vector.find2DAngle(a.z, a.x, b.z, b.x)), Vector.normalizeRadians(Vector.find2DAngle(a.z, a.y, b.z, b.y)), Vector.normalizeRadians(Vector.find2DAngle(a.x, a.y, b.x, b.y)));
        }
        else {
            return new Vector(Vector.find2DAngle(a.z, a.x, b.z, b.x), Vector.find2DAngle(a.z, a.y, b.z, b.y), Vector.find2DAngle(a.x, a.y, b.x, b.y));
        }
    }
    /**
     * Find roll pitch yaw of plane formed by 3 points
     * @param {Vector} a: Vector
     * @param {Vector} b: Vector
     * @param {Vector} c: Vector
     */
    static rollPitchYaw(a, b, c) {
        if (!c) {
            return new Vector(Vector.normalizeAngle(Vector.find2DAngle(a.z, a.y, b.z, b.y)), Vector.normalizeAngle(Vector.find2DAngle(a.z, a.x, b.z, b.x)), Vector.normalizeAngle(Vector.find2DAngle(a.x, a.y, b.x, b.y)));
        }
        let qb = b.subtract(a);
        let qc = c.subtract(a);
        let n = qb.cross(qc);
        let unitZ = n.unit();
        let unitX = qb.unit();
        let unitY = unitZ.cross(unitX);
        let beta = Math.asin(unitZ.x) || 0;
        let alpha = Math.atan2(-unitZ.y, unitZ.z) || 0;
        let gamma = Math.atan2(-unitY.x, unitX.x) || 0;
        return new Vector(Vector.normalizeAngle(alpha), Vector.normalizeAngle(beta), Vector.normalizeAngle(gamma));
    }
    /**
     * Find angle between 3D Coordinates
     * @param {Vector | number} a:
     */
    static angleBetween3DCoords(a, b, c) {
        if (!(a instanceof Vector)) {
            a = new Vector(a);
            b = new Vector(b);
            c = new Vector(c);
        }
        // Calculate vector between points 1 and 2
        const v1 = a.subtract(b);
        // Calculate vector between points 2 and 3
        const v2 = c.subtract(b);
        // The dot product of vectors v1 & v2 is a function of the cosine of the
        // angle between them (it's scaled by the product of their magnitudes).
        const v1norm = v1.unit();
        const v2norm = v2.unit();
        // Calculate the dot products of vectors v1 and v2
        const dotProducts = v1norm.dot(v2norm);
        // Extract the angle from the dot products
        const angle = Math.acos(dotProducts);
        // return single angle Normalized to 1
        return Vector.normalizeRadians(angle);
    }
}
