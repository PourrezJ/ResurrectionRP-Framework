class PlayerPosition { constructor() { this.position = { x: 0, y: 0, z: 0 }, this.update = null, alt.on("playerPosition", (e, t, n) => { this.position.x = e, this.position.y = t, this.position.z = n, this.update && this.update(this.position) }) } getPosition() { return this.position } } var playerPosition = new PlayerPosition, rootProto = void 0, protobufMethod = function (undefined$1) {
    var _Mathmin = Math.min, _Mathpow = Math.pow, _Mathround = Math.round, _Mathfloor = Math.floor, _StringfromCharCode = String.fromCharCode; (function (e, t, n) {// This is the prelude used to bundle protobuf.js for the browser. Wraps up the CommonJS
        // sources through a conflict-free require shim and is again wrapped within an iife that
        // provides a minification-friendly `undefined` var plus a global "use strict" directive
        // so that minification can remove the directives of each module.
        function o(n) { var i = t[n]; return i || e[n][0].call(i = t[n] = { exports: {} }, o, i, i.exports), i.exports } var i = o(n[0]);// Expose globally
        rootProto = i, i.util.global.protobuf = i, "function" == typeof define && define.amd && define(["long"], function (e) { return e && e.isLong && (i.util.Long = e, i.configure()), i }), "object" == typeof module && module && module.exports && (module.exports = i)
    })(/* end of prelude */{
        1: [function (e, t) {/**
         * Callback as used by {@link util.asPromise}.
         * @typedef asPromiseCallback
         * @type {function}
         * @param {Error|null} error Error, if any
         * @param {...*} params Additional arguments
         * @returns {undefined}
         */ /**
         * Returns a promise from a node-style callback function.
         * @memberof util
         * @param {asPromiseCallback} fn Function to call
         * @param {*} ctx Function context
         * @param {...*} params Function arguments
         * @returns {Promise<*>} Promisified function
         */t.exports = function (e, t/*, varargs */) { for (var n = Array(arguments.length - 1), o = 0, i = 2, r =/* insideTryCatch */ /* ifNotSet */ /* endedByRPC */!0; i < arguments.length;)n[o++] = arguments[i++]; return new Promise(function (i, s) { n[o] = function (e/*, varargs */) { if (r) if (r =/* bool     */ !1, e) s(e); else { for (var t = Array(arguments.length - 1), n = 0; n < t.length;)t[n++] = arguments[n]; i.apply(null, t) } }; try { e.apply(t || null, n) } catch (e) { r && (r = !1, s(e)) } }) }
        }, {}], 2: [function (e, t, n) {/**
         * A minimal base64 implementation for number arrays.
         * @memberof util
         * @namespace
         */var o = n;/**
         * Calculates the byte length of a base64 encoded string.
         * @param {string} string Base64 encoded string
         * @returns {number} Byte length
         */o.length = function (e) { var t = e.length; if (!t) return 0; for (var o = 0; 1 < --t % 4 && "=" === e.charAt(t);)++o; return Math.ceil(3 * e.length) / 4 - o };// 65..90, 97..122, 48..57, 43, 47
            for (var r = Array(64), s = Array(123), a = 0; 64 > a;)s[r[a] = 26 > a ? a + 65 : 52 > a ? a + 71 : 62 > a ? a - 4 : 43 | a - 59] = a++;/**
         * Encodes a buffer to a base64 encoded string.
         * @param {Uint8Array} buffer Source buffer
         * @param {number} start Source start
         * @param {number} end Source end
         * @returns {string} Base64 encoded string
         */o.encode = function (e, n, o) {// temporary
                for (var s = null, a = [], d = 0,// output index
                    p = 0,// goto index
                    l, u; n < o;)u = e[n++], 0 === p ? (a[d++] = r[u >> 2], l = (3 & u) << 4, p = 1) : 1 === p ? (a[d++] = r[l | u >> 4], l = (15 & u) << 2, p = 2) : 2 === p ? (a[d++] = r[l | u >> 6], a[d++] = r[63 & u], p = 0) : void 0, 8191 < d && ((s || (s = [])).push(_StringfromCharCode.apply(String, a)), d = 0); return p && (a[d++] = r[l], a[d++] = 61, 1 === p && (a[d++] = 61)), s ? (d && s.push(_StringfromCharCode.apply(String, a.slice(0, d))), s.join("")) : _StringfromCharCode.apply(String, a.slice(0, d))
            };/**
         * Decodes a base64 encoded string to a buffer.
         * @param {string} string Source string
         * @param {Uint8Array} buffer Destination buffer
         * @param {number} offset Destination offset
         * @returns {number} Number of bytes written
         * @throws {Error} If encoding is invalid
         */ /**
         * Tests if the specified string appears to be base64 encoded.
         * @param {string} string String to test
         * @returns {boolean} `true` if probably base64 encoded, otherwise false
         */o.decode = function (e, n, o) {// temporary
                for (var r = o, a = 0, d = 0,// goto index
                    p, l; d < e.length && (l = e.charCodeAt(d++), !(61 === l && 1 < a));) { if ((l = s[l]) === undefined$1) throw Error("invalid encoding"); 0 === a ? (p = l, a = 1) : 1 === a ? (n[o++] = p << 2 | (48 & l) >> 4, p = l, a = 2) : 2 === a ? (n[o++] = (15 & p) << 4 | (60 & l) >> 2, p = l, a = 3) : 3 === a ? (n[o++] = (3 & p) << 6 | l, a = 0) : void 0 } if (1 === a) throw Error("invalid encoding"); return o - r
            }, o.test = function (e) { return /^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$/.test(e) }
        }, {}], 3: [function (e, t) {/**
         * Begins generating a function.
         * @memberof util
         * @param {string[]} functionParams Function parameter names
         * @param {string} [functionName] Function name if not anonymous
         * @returns {Codegen} Appender that appends code to the function's body
         */function n(e, t) {/**
             * Appends code to the function's body or finishes generation.
             * @typedef Codegen
             * @type {function}
             * @param {string|Object.<string,*>} [formatStringOrScope] Format string or, to finish the function, an object of additional scope variables, if any
             * @param {...*} [formatParams] Format parameters
             * @returns {Codegen|Function} Itself or the generated function if finished
             * @throws {Error} If format parameter counts do not match
             */function o(e) {// note that explicit array handling below makes this ~50% faster
                    // finish the function
                    if ("string" != typeof e) {
                        var t = i(); if (n.verbose && console.log("codegen: " + t), t = "return " + t, e) {
                            for (var s = Object.keys(e), a = Array(s.length + 1), d = Array(s.length), p = 0; p < s.length;)a[p] = s[p], d[p] = e[s[p++]]; return a[p] = t, Function.apply(null, a).apply(null, d);// eslint-disable-line no-new-func
                        } return Function(t)();// eslint-disable-line no-new-func
                    }// otherwise append to body
                    for (var l = Array(arguments.length - 1), u = 0; u < l.length;)l[u] = arguments[++u]; if (u = 0, e = e.replace(/%([%dfijs])/g, function (e, t) { var n = l[u++]; return "d" === t || "f" === t ? +n + "" : "i" === t ? _Mathfloor(n) + "" : "j" === t ? JSON.stringify(n) : "s" === t ? n + "" : "%" }), u !== l.length) throw Error("parameter count mismatch"); return r.push(e), o
                } function i(n) { return "function " + (n || t || "") + "(" + (e && e.join(",") || "") + "){\n  " + r.join("\n  ") + "\n}" } "string" == typeof e && (t = e, e = undefined$1); var r = []; return o.toString = i, o
            }/**
         * Begins generating a function.
         * @memberof util
         * @function codegen
         * @param {string} [functionName] Function name if not anonymous
         * @returns {Codegen} Appender that appends code to the function's body
         * @variation 2
         */ /**
         * When set to `true`, codegen will log generated code to console. Useful for debugging.
         * @name util.codegen.verbose
         * @type {boolean}
         */t.exports = n, n.verbose = !1
        }, {}], 4: [function (e, t) {/**
         * Constructs a new event emitter instance.
         * @classdesc A minimal event emitter.
         * @memberof util
         * @constructor
         */function n() {/**
             * Registered listeners.
             * @type {Object.<string,*>}
             * @private
             */this._listeners = {}
            }/**
         * Registers an event listener.
         * @param {string} evt Event name
         * @param {function} fn Listener
         * @param {*} [ctx] Listener context
         * @returns {util.EventEmitter} `this`
         */ /**
         * Removes an event listener or any matching listeners if arguments are omitted.
         * @param {string} [evt] Event name. Removes all listeners if omitted.
         * @param {function} [fn] Listener to remove. Removes all listeners of `evt` if omitted.
         * @returns {util.EventEmitter} `this`
         */ /**
         * Emits an event by calling its listeners with the specified arguments.
         * @param {string} evt Event name
         * @param {...*} args Arguments
         * @returns {util.EventEmitter} `this`
         */t.exports = n, n.prototype.on = function (e, t, n) { return (this._listeners[e] || (this._listeners[e] = [])).push({ fn: t, ctx: n || this }), this }, n.prototype.off = function (e, t) { if (e === undefined$1) this._listeners = {}; else if (t === undefined$1) this._listeners[e] = []; else for (var n = this._listeners[e], o = 0; o < n.length;)n[o].fn === t ? n.splice(o, 1) : ++o; return this }, n.prototype.emit = function (e) { var t = this._listeners[e]; if (t) { for (var n = [], o = 1; o < arguments.length;)n.push(arguments[o++]); for (o = 0; o < t.length;)t[o].fn.apply(t[o++].ctx, n) } return this }
        }, {}], 5: [function (e, t) {/**
         * Node-style callback as used by {@link util.fetch}.
         * @typedef FetchCallback
         * @type {function}
         * @param {?Error} error Error, if any, otherwise `null`
         * @param {string} [contents] File contents, if there hasn't been an error
         * @returns {undefined}
         */ /**
         * Options as used by {@link util.fetch}.
         * @typedef FetchOptions
         * @type {Object}
         * @property {boolean} [binary=false] Whether expecting a binary response
         * @property {boolean} [xhr=false] If `true`, forces the use of XMLHttpRequest
         */ /**
         * Fetches the contents of a file.
         * @memberof util
         * @param {string} filename File path or url
         * @param {FetchOptions} options Fetch options
         * @param {FetchCallback} callback Callback function
         * @returns {undefined}
         */function n(e, t, i) {
                return "function" == typeof t ? (i = t, t = {}) : !t && (t = {}), i ? !t.xhr && r && r.readFile ? r.readFile(e, function (o, r) { return o && "undefined" != typeof XMLHttpRequest ? n.xhr(e, t, i) : o ? i(o) : i(null, t.binary ? r : r.toString("utf8")) }) : n.xhr(e, t, i) : o(n, this, e, t);// eslint-disable-line no-invalid-this
                // if a node-like filesystem is present, try it first but fall back to XHR if nothing is found.
                // use the XHR version otherwise.
            }/**
         * Fetches the contents of a file.
         * @name util.fetch
         * @function
         * @param {string} path File path or url
         * @param {FetchCallback} callback Callback function
         * @returns {undefined}
         * @variation 2
         */ /**
         * Fetches the contents of a file.
         * @name util.fetch
         * @function
         * @param {string} path File path or url
         * @param {FetchOptions} [options] Fetch options
         * @returns {Promise<string|Uint8Array>} Promise
         * @variation 3
         */ /**/t.exports = n; var o = e(1), i = e(7), r = i("fs"); n.xhr = function (e, t, n) {
                var o = new XMLHttpRequest; o.onreadystatechange/* works everywhere */ = function () {
                    if (4 !== o.readyState) return undefined$1;// local cors security errors return status 0 / empty string, too. afaik this cannot be
                    // reliably distinguished from an actually empty file for security reasons. feel free
                    // to send a pull request if you are aware of a solution.
                    if (0 !== o.status && 200 !== o.status) return n(Error("status " + o.status));// if binary data is expected, make sure that some sort of array is returned, even if
                    // ArrayBuffers are not supported. the binary string fallback, however, is unsafe.
                    if (t.binary) { var e = o.response; if (!e) { e = []; for (var r = 0; r < o.responseText.length; ++r)e.push(255 & o.responseText.charCodeAt(r)) } return n(null, "undefined" == typeof Uint8Array ? e : new Uint8Array(e)) } return n(null, o.responseText)
                }, t.binary && ("overrideMimeType" in o && o.overrideMimeType("text/plain; charset=x-user-defined"), o.responseType = "arraybuffer"), o.open("GET", e), o.send()
            }
        }, { 1: 1, 7: 7 }], 6: [function (e, t) {/**
         * Reads / writes floats / doubles from / to buffers.
         * @name util.float
         * @namespace
         */ /**
         * Writes a 32 bit float to a buffer using little endian byte order.
         * @name util.float.writeFloatLE
         * @function
         * @param {number} val Value to write
         * @param {Uint8Array} buf Target buffer
         * @param {number} pos Target buffer offset
         * @returns {undefined}
         */ /**
         * Writes a 32 bit float to a buffer using big endian byte order.
         * @name util.float.writeFloatBE
         * @function
         * @param {number} val Value to write
         * @param {Uint8Array} buf Target buffer
         * @param {number} pos Target buffer offset
         * @returns {undefined}
         */ /**
         * Reads a 32 bit float from a buffer using little endian byte order.
         * @name util.float.readFloatLE
         * @function
         * @param {Uint8Array} buf Source buffer
         * @param {number} pos Source buffer offset
         * @returns {number} Value read
         */ /**
         * Reads a 32 bit float from a buffer using big endian byte order.
         * @name util.float.readFloatBE
         * @function
         * @param {Uint8Array} buf Source buffer
         * @param {number} pos Source buffer offset
         * @returns {number} Value read
         */ /**
         * Writes a 64 bit double to a buffer using little endian byte order.
         * @name util.float.writeDoubleLE
         * @function
         * @param {number} val Value to write
         * @param {Uint8Array} buf Target buffer
         * @param {number} pos Target buffer offset
         * @returns {undefined}
         */ /**
         * Writes a 64 bit double to a buffer using big endian byte order.
         * @name util.float.writeDoubleBE
         * @function
         * @param {number} val Value to write
         * @param {Uint8Array} buf Target buffer
         * @param {number} pos Target buffer offset
         * @returns {undefined}
         */ /**
         * Reads a 64 bit double from a buffer using little endian byte order.
         * @name util.float.readDoubleLE
         * @function
         * @param {Uint8Array} buf Source buffer
         * @param {number} pos Source buffer offset
         * @returns {number} Value read
         */ /**
         * Reads a 64 bit double from a buffer using big endian byte order.
         * @name util.float.readDoubleBE
         * @function
         * @param {Uint8Array} buf Source buffer
         * @param {number} pos Source buffer offset
         * @returns {number} Value read
         */ // Factory function for the purpose of node-based testing in modified global environments
            function n(e) {
                var t = Math.LN2, n = Math.log; return "undefined" == typeof Float32Array ? function () {
                    function a(e, o, i, r) {
                        var s = 0 > o ? 1 : 0; if (s && (o = -o), 0 === o) e(0 < 1 / o ?/* positive */0 :/* negative 0 */2147483648, i, r); else if (isNaN(o)) e(2143289344, i, r); else if (34028234663852886e22 < o)// +-Infinity
                            e((2139095040 | s << 31) >>> 0, i, r); else if (11754943508222875e-54 > o)// denormal
                            e((s << 31 | _Mathround(o / 1401298464324817e-60)) >>> 0, i, r); else { var a = _Mathfloor(n(o) / t), d = 8388607 & _Mathround(8388608 * (o * _Mathpow(2, -a))); e((s << 31 | a + 127 << 23 | d) >>> 0, i, r) }
                    } function d(e, t, n) {
                        var o = e(t, n), i = 2 * (o >> 31) + 1, r = 255 & o >>> 23, s = 8388607 & o; return 255 == r ? s ? NaN : i * (1 / 0) : 0 == r// denormal
                            ? 1401298464324817e-60 * i * s : i * _Mathpow(2, r - 150) * (s + 8388608)
                    } e.writeFloatLE = a.bind(null, o), e.writeFloatBE = a.bind(null, i), e.readFloatLE = d.bind(null, r), e.readFloatBE = d.bind(null, s)
                }() : function () { function t(e, t, n) { r[0] = e, t[n] = s[0], t[n + 1] = s[1], t[n + 2] = s[2], t[n + 3] = s[3] } function n(e, t, n) { r[0] = e, t[n] = s[3], t[n + 1] = s[2], t[n + 2] = s[1], t[n + 3] = s[0] }/* istanbul ignore next */function o(e, t) { return s[0] = e[t], s[1] = e[t + 1], s[2] = e[t + 2], s[3] = e[t + 3], r[0] } function i(e, t) { return s[3] = e[t], s[2] = e[t + 1], s[1] = e[t + 2], s[0] = e[t + 3], r[0] }/* istanbul ignore next */var r = new Float32Array([-0]), s = new Uint8Array(r.buffer), a = 128 === s[3]; e.writeFloatLE = a ? t : n, e.writeFloatBE = a ? n : t, e.readFloatLE = a ? o : i, e.readFloatBE = a ? i : o }(), "undefined" == typeof Float64Array ? function () {
                    function a(e, o, i, r, s, a) { var d = 0 > r ? 1 : 0; if (d && (r = -r), 0 === r) e(0, s, a + o), e(0 < 1 / r ?/* positive */0 :/* negative 0 */2147483648, s, a + i); else if (isNaN(r)) e(0, s, a + o), e(2146959360, s, a + i); else if (17976931348623157e292 < r) e(0, s, a + o), e((2146435072 | d << 31) >>> 0, s, a + i); else { var p; if (22250738585072014e-324 > r) p = r / 5e-324, e(p >>> 0, s, a + o), e((d << 31 | p / 4294967296) >>> 0, s, a + i); else { var l = _Mathfloor(n(r) / t); 1024 === l && (l = 1023), p = r * _Mathpow(2, -l), e(4503599627370496 * p >>> 0, s, a + o), e((d << 31 | l + 1023 << 20 | 1048575 & 1048576 * p) >>> 0, s, a + i) } } } function d(e, t, n, o, i) {
                        var r = e(o, i + t), s = e(o, i + n), a = 2 * (s >> 31) + 1, d = 2047 & s >>> 20, p = 4294967296 * (1048575 & s) + r; return 2047 === d ? p ? NaN : a * (1 / 0) : 0 === d// denormal
                            ? 5e-324 * a * p : a * _Mathpow(2, d - 1075) * (p + 4503599627370496)
                    } e.writeDoubleLE = a.bind(null, o, 0, 4), e.writeDoubleBE = a.bind(null, i, 4, 0), e.readDoubleLE = d.bind(null, r, 0, 4), e.readDoubleBE = d.bind(null, s, 4, 0)
                }() : function () { function t(e, t, n) { r[0] = e, t[n] = s[0], t[n + 1] = s[1], t[n + 2] = s[2], t[n + 3] = s[3], t[n + 4] = s[4], t[n + 5] = s[5], t[n + 6] = s[6], t[n + 7] = s[7] } function n(e, t, n) { r[0] = e, t[n] = s[7], t[n + 1] = s[6], t[n + 2] = s[5], t[n + 3] = s[4], t[n + 4] = s[3], t[n + 5] = s[2], t[n + 6] = s[1], t[n + 7] = s[0] }/* istanbul ignore next */function o(e, t) { return s[0] = e[t], s[1] = e[t + 1], s[2] = e[t + 2], s[3] = e[t + 3], s[4] = e[t + 4], s[5] = e[t + 5], s[6] = e[t + 6], s[7] = e[t + 7], r[0] } function i(e, t) { return s[7] = e[t], s[6] = e[t + 1], s[5] = e[t + 2], s[4] = e[t + 3], s[3] = e[t + 4], s[2] = e[t + 5], s[1] = e[t + 6], s[0] = e[t + 7], r[0] }/* istanbul ignore next */var r = new Float64Array([-0]), s = new Uint8Array(r.buffer), a = 128 === s[7]; e.writeDoubleLE = a ? t : n, e.writeDoubleBE = a ? n : t, e.readDoubleLE = a ? o : i, e.readDoubleBE = a ? i : o }(), e
            }// uint helpers
            function o(e, t, n) { t[n] = 255 & e, t[n + 1] = 255 & e >>> 8, t[n + 2] = 255 & e >>> 16, t[n + 3] = e >>> 24 } function i(e, t, n) { t[n] = e >>> 24, t[n + 1] = 255 & e >>> 16, t[n + 2] = 255 & e >>> 8, t[n + 3] = 255 & e } function r(e, t) { return (e[t] | e[t + 1] << 8 | e[t + 2] << 16 | e[t + 3] << 24) >>> 0 } function s(e, t) { return (e[t] << 24 | e[t + 1] << 16 | e[t + 2] << 8 | e[t + 3]) >>> 0 } t.exports = n(n)
        }, {}], 7: [function (require, module, exports) {/**
         * Requires a module only if available.
         * @memberof util
         * @param {string} moduleName Module to require
         * @returns {?Object} Required module if available and not empty, otherwise `null`
         */function inquire(moduleName) {
                try {
                    var mod = eval("quire".replace(/^/, "re"))(moduleName);// eslint-disable-line no-eval
                    if (mod && (mod.length || Object.keys(mod).length)) return mod
                } catch (t) { }// eslint-disable-line no-empty
                return null
            } module.exports = inquire
        }, {}], 8: [function (e, t, n) {/**
         * A minimal path module to resolve Unix, Windows and URL paths alike.
         * @memberof util
         * @namespace
         */var o = n, r =/**
             * Tests if the specified path is absolute.
             * @param {string} path Path to test
             * @returns {boolean} `true` if path is absolute
             */o.isAbsolute = function (e) { return /^(?:\/|\w+:)/.test(e) }, s =/**
             * Normalizes the specified path.
             * @param {string} path Path to normalize
             * @returns {string} Normalized path
             */o.normalize = function (e) { e = e.replace(/\\/g, "/").replace(/\/{2,}/g, "/"); var t = e.split("/"), n = r(e), o = ""; n && (o = t.shift() + "/"); for (var s = 0; s < t.length;)".." === t[s] ? 0 < s && ".." !== t[s - 1] ? t.splice(--s, 2) : n ? t.splice(s, 1) : ++s : "." === t[s] ? t.splice(s, 1) : ++s; return o + t.join("/") };/**
         * Resolves the specified include path against the specified origin path.
         * @param {string} originPath Path to the origin file
         * @param {string} includePath Include path relative to origin path
         * @param {boolean} [alreadyNormalized=false] `true` if both paths are already known to be normalized
         * @returns {string} Path to the include file
         */o.resolve = function (e, t, n) { return (n || (t = s(t)), r(t)) ? t : (n || (e = s(e)), (e = e.replace(/(?:\/|^)[^/]+$/, "")).length ? s(e + "/" + t) : t) }
        }, {}], 9: [function (e, t) {/**
         * An allocator as used by {@link util.pool}.
         * @typedef PoolAllocator
         * @type {function}
         * @param {number} size Buffer size
         * @returns {Uint8Array} Buffer
         */ /**
         * A slicer as used by {@link util.pool}.
         * @typedef PoolSlicer
         * @type {function}
         * @param {number} start Start offset
         * @param {number} end End offset
         * @returns {Uint8Array} Buffer slice
         * @this {Uint8Array}
         */ /**
         * A general purpose buffer pool.
         * @memberof util
         * @function
         * @param {PoolAllocator} alloc Allocator
         * @param {PoolSlicer} slice Slicer
         * @param {number} [size=8192] Slab size
         * @returns {PoolAllocator} Pooled allocator
         */t.exports = function (e, t, n) {
                var o = n || 8192, i = null, r = o; return function (n) {
                    if (1 > n || n > o >>> 1) return e(n); r + n > o && (i = e(o), r = 0); var s = t.call(i, r, r += n); return 7 & r && (// align to 32 bit
                        r = (7 | r) + 1), s
                }
            }
        }, {}], 10: [function (e, t, n) {/**
         * A minimal UTF8 implementation for number arrays.
         * @memberof util
         * @namespace
         */var o = n;/**
         * Calculates the UTF8 byte length of a string.
         * @param {string} string String
         * @returns {number} Byte length
         */ /**
         * Reads UTF8 bytes as a string.
         * @param {Uint8Array} buffer Source buffer
         * @param {number} start Source start
         * @param {number} end Source end
         * @returns {string} String read
         */ /**
         * Writes a string as UTF8 bytes.
         * @param {string} string Source string
         * @param {Uint8Array} buffer Destination buffer
         * @param {number} offset Destination offset
         * @returns {number} Bytes written
         */o.length = function (e) { for (var t = 0, n = 0, o = 0; o < e.length; ++o)n = e.charCodeAt(o), 128 > n ? t += 1 : 2048 > n ? t += 2 : 55296 == (64512 & n) && 56320 == (64512 & e.charCodeAt(o + 1)) ? (++o, t += 4) : t += 3; return t }, o.read = function (e, n, o) {
                var r = o - n; if (1 > r) return "";// temporary
                for (var s = null, a = [], d = 0,// char offset
                    p; n < o;)p = e[n++], 128 > p ? a[d++] = p : 191 < p && 224 > p ? a[d++] = (31 & p) << 6 | 63 & e[n++] : 239 < p && 365 > p ? (p = ((7 & p) << 18 | (63 & e[n++]) << 12 | (63 & e[n++]) << 6 | 63 & e[n++]) - 65536, a[d++] = 55296 + (p >> 10), a[d++] = 56320 + (1023 & p)) : a[d++] = (15 & p) << 12 | (63 & e[n++]) << 6 | 63 & e[n++], 8191 < d && ((s || (s = [])).push(_StringfromCharCode.apply(String, a)), d = 0); return s ? (d && s.push(_StringfromCharCode.apply(String, a.slice(0, d))), s.join("")) : _StringfromCharCode.apply(String, a.slice(0, d))
            }, o.write = function (e, t, n) {// character 2
                for (var o = n, r = 0, s,// character 1
                    a; r < e.length; ++r)s = e.charCodeAt(r), 128 > s ? t[n++] = s : 2048 > s ? (t[n++] = 192 | s >> 6, t[n++] = 128 | 63 & s) : 55296 == (64512 & s) && 56320 == (64512 & (a = e.charCodeAt(r + 1))) ? (s = 65536 + ((1023 & s) << 10) + (1023 & a), ++r, t[n++] = 240 | s >> 18, t[n++] = 128 | 63 & s >> 12, t[n++] = 128 | 63 & s >> 6, t[n++] = 128 | 63 & s) : (t[n++] = 224 | s >> 12, t[n++] = 128 | 63 & s >> 6, t[n++] = 128 | 63 & s); return n - o
            }
        }, {}], 11: [function (e, t) {/**
         * Provides common type definitions.
         * Can also be used to provide additional google types or your own custom types.
         * @param {string} name Short name as in `google/protobuf/[name].proto` or full file name
         * @param {Object.<string,*>} json JSON definition within `google.protobuf` if a short name, otherwise the file's root definition
         * @returns {undefined}
         * @property {INamespace} google/protobuf/any.proto Any
         * @property {INamespace} google/protobuf/duration.proto Duration
         * @property {INamespace} google/protobuf/empty.proto Empty
         * @property {INamespace} google/protobuf/field_mask.proto FieldMask
         * @property {INamespace} google/protobuf/struct.proto Struct, Value, NullValue and ListValue
         * @property {INamespace} google/protobuf/timestamp.proto Timestamp
         * @property {INamespace} google/protobuf/wrappers.proto Wrappers
         * @example
         * // manually provides descriptor.proto (assumes google/protobuf/ namespace and .proto extension)
         * protobuf.common("descriptor", descriptorJson);
         *
         * // manually provides a custom definition (uses my.foo namespace)
         * protobuf.common("my/foo/bar.proto", myFooBarJson);
         */function n(e, t) { o.test(e) || (e = "google/protobuf/" + e + ".proto", t = { nested: { google: { nested: { protobuf: { nested: t } } } } }), n[e] = t }// Not provided because of limited use (feel free to discuss or to provide yourself):
            //
            // google/protobuf/descriptor.proto
            // google/protobuf/source_context.proto
            // google/protobuf/type.proto
            //
            // Stripped and pre-parsed versions of these non-bundled files are instead available as part of
            // the repository or package within the google/protobuf directory.
            t.exports = n; var o = /\/|\./; n("any", {/**
             * Properties of a google.protobuf.Any message.
             * @interface IAny
             * @type {Object}
             * @property {string} [typeUrl]
             * @property {Uint8Array} [bytes]
             * @memberof common
             */Any: { fields: { type_url: { type: "string", id: 1 }, value: { type: "bytes", id: 2 } } }
            }); var i;/**
         * Gets the root definition of the specified common proto file.
         *
         * Bundled definitions are:
         * - google/protobuf/any.proto
         * - google/protobuf/duration.proto
         * - google/protobuf/empty.proto
         * - google/protobuf/field_mask.proto
         * - google/protobuf/struct.proto
         * - google/protobuf/timestamp.proto
         * - google/protobuf/wrappers.proto
         *
         * @param {string} file Proto file name
         * @returns {INamespace|null} Root definition or `null` if not defined
         */n("duration", {/**
             * Properties of a google.protobuf.Duration message.
             * @interface IDuration
             * @type {Object}
             * @property {number|Long} [seconds]
             * @property {number} [nanos]
             * @memberof common
             */Duration: i = { fields: { seconds: { type: "int64", id: 1 }, nanos: { type: "int32", id: 2 } } }
            }), n("timestamp", {/**
             * Properties of a google.protobuf.Timestamp message.
             * @interface ITimestamp
             * @type {Object}
             * @property {number|Long} [seconds]
             * @property {number} [nanos]
             * @memberof common
             */Timestamp: i
            }), n("empty", {/**
             * Properties of a google.protobuf.Empty message.
             * @interface IEmpty
             * @memberof common
             */Empty: { fields: {} }
            }), n("struct", {/**
             * Properties of a google.protobuf.Struct message.
             * @interface IStruct
             * @type {Object}
             * @property {Object.<string,IValue>} [fields]
             * @memberof common
             */Struct: { fields: { fields: { keyType: "string", type: "Value", id: 1 } } },/**
             * Properties of a google.protobuf.Value message.
             * @interface IValue
             * @type {Object}
             * @property {string} [kind]
             * @property {0} [nullValue]
             * @property {number} [numberValue]
             * @property {string} [stringValue]
             * @property {boolean} [boolValue]
             * @property {IStruct} [structValue]
             * @property {IListValue} [listValue]
             * @memberof common
             */Value: { oneofs: { kind: { oneof: ["nullValue", "numberValue", "stringValue", "boolValue", "structValue", "listValue"] } }, fields: { nullValue: { type: "NullValue", id: 1 }, numberValue: { type: "double", id: 2 }, stringValue: { type: "string", id: 3 }, boolValue: { type: "bool", id: 4 }, structValue: { type: "Struct", id: 5 }, listValue: { type: "ListValue", id: 6 } } }, NullValue: { values: { NULL_VALUE: 0 } },/**
             * Properties of a google.protobuf.ListValue message.
             * @interface IListValue
             * @type {Object}
             * @property {Array.<IValue>} [values]
             * @memberof common
             */ListValue: { fields: { values: { rule: "repeated", type: "Value", id: 1 } } }
            }), n("wrappers", {/**
             * Properties of a google.protobuf.DoubleValue message.
             * @interface IDoubleValue
             * @type {Object}
             * @property {number} [value]
             * @memberof common
             */DoubleValue: { fields: { value: { type: "double", id: 1 } } },/**
             * Properties of a google.protobuf.FloatValue message.
             * @interface IFloatValue
             * @type {Object}
             * @property {number} [value]
             * @memberof common
             */FloatValue: { fields: { value: { type: "float", id: 1 } } },/**
             * Properties of a google.protobuf.Int64Value message.
             * @interface IInt64Value
             * @type {Object}
             * @property {number|Long} [value]
             * @memberof common
             */Int64Value: { fields: { value: { type: "int64", id: 1 } } },/**
             * Properties of a google.protobuf.UInt64Value message.
             * @interface IUInt64Value
             * @type {Object}
             * @property {number|Long} [value]
             * @memberof common
             */UInt64Value: { fields: { value: { type: "uint64", id: 1 } } },/**
             * Properties of a google.protobuf.Int32Value message.
             * @interface IInt32Value
             * @type {Object}
             * @property {number} [value]
             * @memberof common
             */Int32Value: { fields: { value: { type: "int32", id: 1 } } },/**
             * Properties of a google.protobuf.UInt32Value message.
             * @interface IUInt32Value
             * @type {Object}
             * @property {number} [value]
             * @memberof common
             */UInt32Value: { fields: { value: { type: "uint32", id: 1 } } },/**
             * Properties of a google.protobuf.BoolValue message.
             * @interface IBoolValue
             * @type {Object}
             * @property {boolean} [value]
             * @memberof common
             */BoolValue: { fields: { value: { type: "bool", id: 1 } } },/**
             * Properties of a google.protobuf.StringValue message.
             * @interface IStringValue
             * @type {Object}
             * @property {string} [value]
             * @memberof common
             */StringValue: { fields: { value: { type: "string", id: 1 } } },/**
             * Properties of a google.protobuf.BytesValue message.
             * @interface IBytesValue
             * @type {Object}
             * @property {Uint8Array} [value]
             * @memberof common
             */BytesValue: { fields: { value: { type: "bytes", id: 1 } } }
            }), n("field_mask", {/**
             * Properties of a google.protobuf.FieldMask message.
             * @interface IDoubleValue
             * @type {Object}
             * @property {number} [value]
             * @memberof common
             */FieldMask: { fields: { paths: { rule: "repeated", type: "string", id: 1 } } }
            }), n.get = function (e) { return n[e] || null }
        }, {}], 12: [function (e, t, n) {/**
         * Generates a partial value fromObject conveter.
         * @param {Codegen} gen Codegen instance
         * @param {Field} field Reflected field
         * @param {number} fieldIndex Field index
         * @param {string} prop Property reference
         * @returns {Codegen} Codegen instance
         * @ignore
         */function o(e, t, n, o) {/* eslint-disable no-unexpected-multiline, block-scoped-var, no-redeclare */if (!t.resolvedType) {
                var r = !1; switch (t.type) {
                    case "double": case "float": e("m%s=Number(d%s)", o, o);// also catches "NaN", "Infinity"
                        break; case "uint32": case "fixed32": e("m%s=d%s>>>0", o, o); break; case "int32": case "sint32": case "sfixed32": e("m%s=d%s|0", o, o); break; case "uint64": r = !0;// eslint-disable-line no-fallthrough
                    case "int64": case "sint64": case "fixed64": case "sfixed64": e("if(util.Long)")("(m%s=util.Long.fromValue(d%s)).unsigned=%j", o, o, r)("else if(typeof d%s===\"string\")", o)("m%s=parseInt(d%s,10)", o, o)("else if(typeof d%s===\"number\")", o)("m%s=d%s", o, o)("else if(typeof d%s===\"object\")", o)("m%s=new util.LongBits(d%s.low>>>0,d%s.high>>>0).toNumber(%s)", o, o, o, r ? "true" : ""); break; case "bytes": e("if(typeof d%s===\"string\")", o)("util.base64.decode(d%s,m%s=util.newBuffer(util.base64.length(d%s)),0)", o, o, o)("else if(d%s.length)", o)("m%s=d%s", o, o); break; case "string": e("m%s=String(d%s)", o, o); break; case "bool": e("m%s=Boolean(d%s)", o, o);/* default: gen
                ("m%s=d%s", prop, prop);
                break; */}
            } else if (t.resolvedType instanceof a) { e("switch(d%s){", o); for (var s = t.resolvedType.values, d = Object.keys(s), p = 0; p < d.length; ++p)t.repeated && s[d[p]] === t.typeDefault && e("default:"), e("case%j:", d[p])("case %i:", s[d[p]])("m%s=%j", o, s[d[p]])("break"); e("}") } else e("if(typeof d%s!==\"object\")", o)("throw TypeError(%j)", t.fullName + ": object expected")("m%s=types[%i].fromObject(d%s)", o, n, o); return e;/* eslint-enable no-unexpected-multiline, block-scoped-var, no-redeclare */
            }/**
         * Generates a plain object to runtime message converter specific to the specified message type.
         * @param {Type} mtype Message type
         * @returns {Codegen} Codegen instance
         */ /**
         * Generates a partial value toObject converter.
         * @param {Codegen} gen Codegen instance
         * @param {Field} field Reflected field
         * @param {number} fieldIndex Field index
         * @param {string} prop Property reference
         * @returns {Codegen} Codegen instance
         * @ignore
         */function r(e, t, n, o) {/* eslint-disable no-unexpected-multiline, block-scoped-var, no-redeclare */if (t.resolvedType) t.resolvedType instanceof a ? e("d%s=o.enums===String?types[%i].values[m%s]:m%s", o, n, o, o) : e("d%s=types[%i].toObject(m%s,o)", o, n, o); else {
                var i = !1; switch (t.type) {
                    case "double": case "float": e("d%s=o.json&&!isFinite(m%s)?String(m%s):m%s", o, o, o, o); break; case "uint64": i = !0;// eslint-disable-line no-fallthrough
                    case "int64": case "sint64": case "fixed64": case "sfixed64": e("if(typeof m%s===\"number\")", o)("d%s=o.longs===String?String(m%s):m%s", o, o, o)("else")// Long-like
                        ("d%s=o.longs===String?util.Long.prototype.toString.call(m%s):o.longs===Number?new util.LongBits(m%s.low>>>0,m%s.high>>>0).toNumber(%s):m%s", o, o, o, o, i ? "true" : "", o); break; case "bytes": e("d%s=o.bytes===String?util.base64.encode(m%s,0,m%s.length):o.bytes===Array?Array.prototype.slice.call(m%s):m%s", o, o, o, o, o); break; default: e("d%s=m%s", o, o);
                }
            } return e;/* eslint-enable no-unexpected-multiline, block-scoped-var, no-redeclare */
            }/**
         * Generates a runtime message to plain object converter specific to the specified message type.
         * @param {Type} mtype Message type
         * @returns {Codegen} Codegen instance
         */ /**
         * Runtime message from/to plain object converters.
         * @namespace
         */var s = n, a = e(15), d = e(37); s.fromObject = function (e) {/* eslint-disable no-unexpected-multiline, block-scoped-var, no-redeclare */var t = e.fieldsArray, n = d.codegen(["d"], e.name + "$fromObject")("if(d instanceof this.ctor)")("return d"); if (!t.length) return n("return new this.ctor"); n("var m=new this.ctor"); for (var r = 0; r < t.length; ++r) {
                var s = t[r].resolve(), p = d.safeProp(s.name);// Map fields
                s.map ? (n("if(d%s){", p)("if(typeof d%s!==\"object\")", p)("throw TypeError(%j)", s.fullName + ": object expected")("m%s={}", p)("for(var ks=Object.keys(d%s),i=0;i<ks.length;++i){", p), o(n, s,/* not sorted */r, p + "[ks[i]]")("}")("}")) : s.repeated ? (n("if(d%s){", p)("if(!Array.isArray(d%s))", p)("throw TypeError(%j)", s.fullName + ": array expected")("m%s=[]", p)("for(var i=0;i<d%s.length;++i){", p), o(n, s,/* not sorted */r, p + "[i]")("}")("}")) : (!(s.resolvedType instanceof a) && n// no need to test for null/undefined if an enum (uses switch)
                    ("if(d%s!=null){", p), o(n, s,/* not sorted */r, p), !(s.resolvedType instanceof a) && n("}"))
            } return n("return m");/* eslint-enable no-unexpected-multiline, block-scoped-var, no-redeclare */
            }, s.toObject = function (e) {/* eslint-disable no-unexpected-multiline, block-scoped-var, no-redeclare */var t = e.fieldsArray.slice().sort(d.compareFieldsById); if (!t.length) return d.codegen()("return {}"); for (var n = d.codegen(["m", "o"], e.name + "$toObject")("if(!o)")("o={}")("var d={}"), o = [], s = [], p = [], l = 0; l < t.length; ++l)t[l].partOf || (t[l].resolve().repeated ? o : t[l].map ? s : p).push(t[l]); if (o.length) { for (n("if(o.arrays||o.defaults){"), l = 0; l < o.length; ++l)n("d%s=[]", d.safeProp(o[l].name)); n("}") } if (s.length) { for (n("if(o.objects||o.defaults){"), l = 0; l < s.length; ++l)n("d%s={}", d.safeProp(s[l].name)); n("}") } if (p.length) {
                for (n("if(o.defaults){"), l = 0; l < p.length; ++l) {
                    var u = p[l], y = d.safeProp(u.name); if (u.resolvedType instanceof a) n("d%s=o.enums===String?%j:%j", y, u.resolvedType.valuesById[u.typeDefault], u.typeDefault); else if (u.long) n("if(util.Long){")("var n=new util.Long(%i,%i,%j)", u.typeDefault.low, u.typeDefault.high, u.typeDefault.unsigned)("d%s=o.longs===String?n.toString():o.longs===Number?n.toNumber():n", y)("}else")("d%s=o.longs===String?%j:%i", y, u.typeDefault.toString(), u.typeDefault.toNumber()); else if (u.bytes) { var f = "[" + Array.prototype.slice.call(u.typeDefault).join(",") + "]"; n("if(o.bytes===String)d%s=%j", y, _StringfromCharCode.apply(String, u.typeDefault))("else{")("d%s=%s", y, f)("if(o.bytes!==Array)d%s=util.newBuffer(d%s)", y, y)("}") } else n("d%s=%j", y, u.typeDefault);// also messages (=null)
                } n("}")
            } var m = !1; for (l = 0; l < t.length; ++l) { var u = t[l], c = e._fieldsArray.indexOf(u), y = d.safeProp(u.name); u.map ? (!m && (m = !0, n("var ks2")), n("if(m%s&&(ks2=Object.keys(m%s)).length){", y, y)("d%s={}", y)("for(var j=0;j<ks2.length;++j){"), r(n, u,/* sorted */c, y + "[ks2[j]]")("}")) : u.repeated ? (n("if(m%s&&m%s.length){", y, y)("d%s=[]", y)("for(var j=0;j<m%s.length;++j){", y), r(n, u,/* sorted */c, y + "[j]")("}")) : (n("if(m%s!=null&&m.hasOwnProperty(%j)){", y, u.name), r(n, u,/* sorted */c, y), u.partOf && n("if(o.oneofs)")("d%s=%j", d.safeProp(u.partOf.name), u.name)), n("}") } return n("return d");/* eslint-enable no-unexpected-multiline, block-scoped-var, no-redeclare */
            }
        }, { 15: 15, 37: 37 }], 13: [function (e, t) {
            function n(e) { return "missing required '" + e.name + "'" }/**
         * Generates a decoder specific to the specified message type.
         * @param {Type} mtype Message type
         * @returns {Codegen} Codegen instance
         */t.exports = function (e) {/* eslint-disable no-unexpected-multiline */var t = s.codegen(["r", "l"], e.name + "$decode")("if(!(r instanceof Reader))")("r=Reader.create(r)")("var c=l===undefined?r.len:r.pos+l,m=new this.ctor" + (e.fieldsArray.filter(function (e) { return e.map }).length ? ",k" : ""))("while(r.pos<c){")("var t=r.uint32()"); e.group && t("if((t&7)===4)")("break"), t("switch(t>>>3){"); for (var a = 0; a </* initializes */e.fieldsArray.length; ++a) {
                var d = e._fieldsArray[a].resolve(), p = d.resolvedType instanceof o ? "int32" : d.type, l = "m" + s.safeProp(d.name); t("case %i:", d.id), d.map ? (t("r.skip().pos++")// assumes id 1 + key wireType
                    ("if(%s===util.emptyObject)", l)("%s={}", l)("k=r.%s()", d.keyType)("r.pos++"), r.long[d.keyType] === undefined$1 ? r.basic[p] === undefined$1 ? t("%s[k]=types[%i].decode(r,r.uint32())", l, a) :// can't be groups
                        t("%s[k]=r.%s()", l, p) : r.basic[p] === undefined$1 ? t("%s[typeof k===\"object\"?util.longToHash(k):k]=types[%i].decode(r,r.uint32())", l, a) :// can't be groups
                            t("%s[typeof k===\"object\"?util.longToHash(k):k]=r.%s()", l, p)) : d.repeated ? (t("if(!(%s&&%s.length))", l, l)("%s=[]", l), r.packed[p] !== undefined$1 && t("if((t&7)===2){")("var c2=r.uint32()+r.pos")("while(r.pos<c2)")("%s.push(r.%s())", l, p)("}else"), r.basic[p] === undefined$1 ? t(d.resolvedType.group ? "%s.push(types[%i].decode(r))" : "%s.push(types[%i].decode(r,r.uint32()))", l, a) : t("%s.push(r.%s())", l, p)) : r.basic[p] === undefined$1 ? t(d.resolvedType.group ? "%s=types[%i].decode(r)" : "%s=types[%i].decode(r,r.uint32())", l, a) : t("%s=r.%s()", l, p), t("break")
            }// Field presence
                for (t("default:")("r.skipType(t&7)")("break")("}")("}"), a = 0; a < e._fieldsArray.length; ++a) { var u = e._fieldsArray[a]; u.required && t("if(!m.hasOwnProperty(%j))", u.name)("throw util.ProtocolError(%j,{instance:m})", n(u)) } return t("return m");/* eslint-enable no-unexpected-multiline */
            }; var o = e(15), r = e(36), s = e(37)
        }, { 15: 15, 36: 36, 37: 37 }], 14: [function (e, t) {/**
         * Generates a partial message type encoder.
         * @param {Codegen} gen Codegen instance
         * @param {Field} field Reflected field
         * @param {number} fieldIndex Field index
         * @param {string} ref Variable reference
         * @returns {Codegen} Codegen instance
         * @ignore
         */function n(e, t, n, o) { return t.resolvedType.group ? e("types[%i].encode(%s,w.uint32(%i)).uint32(%i)", n, o, (3 | t.id << 3) >>> 0, (4 | t.id << 3) >>> 0) : e("types[%i].encode(%s,w.uint32(%i).fork()).ldelim()", n, o, (2 | t.id << 3) >>> 0) }/**
         * Generates an encoder specific to the specified message type.
         * @param {Type} mtype Message type
         * @returns {Codegen} Codegen instance
         */t.exports = function (e) {/* eslint-disable no-unexpected-multiline, block-scoped-var, no-redeclare */for (var t = s.codegen(["m", "w"], e.name + "$encode")("if(!w)")("w=Writer.create()"), a =/* initializes */e.fieldsArray.slice().sort(s.compareFieldsById), d = 0, d, p; d < a.length; ++d) {
                var l = a[d].resolve(), u = e._fieldsArray.indexOf(l), y = l.resolvedType instanceof o ? "int32" : l.type, f = r.basic[y]; p = "m" + s.safeProp(l.name), l.map ? (t("if(%s!=null&&m.hasOwnProperty(%j)){", p, l.name)// !== undefined && !== null
                    ("for(var ks=Object.keys(%s),i=0;i<ks.length;++i){", p)("w.uint32(%i).fork().uint32(%i).%s(ks[i])", (2 | l.id << 3) >>> 0, 8 | r.mapKey[l.keyType], l.keyType), f === undefined$1 ? t("types[%i].encode(%s[ks[i]],w.uint32(18).fork()).ldelim().ldelim()", u, p) :// can't be groups
                        t(".uint32(%i).%s(%s[ks[i]]).ldelim()", 16 | f, y, p), t("}")("}")) : l.repeated ? (t("if(%s!=null&&%s.length){", p, p), l.packed && r.packed[y] !== undefined$1 ? t("w.uint32(%i).fork()", (2 | l.id << 3) >>> 0)("for(var i=0;i<%s.length;++i)", p)("w.%s(%s[i])", y, p)("w.ldelim()") : (t("for(var i=0;i<%s.length;++i)", p), f === undefined$1 ? n(t, l, u, p + "[i]") : t("w.uint32(%i).%s(%s[i])", (l.id << 3 | f) >>> 0, y, p)), t("}")) : (l.optional && t("if(%s!=null&&m.hasOwnProperty(%j))", p, l.name), f === undefined$1 ? n(t, l, u, p) : t("w.uint32(%i).%s(%s)", (l.id << 3 | f) >>> 0, y, p))
            } return t("return w");/* eslint-enable no-unexpected-multiline, block-scoped-var, no-redeclare */
            }; var o = e(15), r = e(36), s = e(37)
        }, { 15: 15, 36: 36, 37: 37 }], 15: [function (e, t) {/**
         * Constructs a new enum instance.
         * @classdesc Reflected enum.
         * @extends ReflectionObject
         * @constructor
         * @param {string} name Unique name within its namespace
         * @param {Object.<string,number>} [values] Enum values as an object, by name
         * @param {Object.<string,*>} [options] Declared options
         * @param {string} [comment] The comment for this enum
         * @param {Object.<string,string>} [comments] The value comments for this enum
         */function n(e, t, n, r, s) {
                if (o.call(this, e, n), t && "object" != typeof t) throw TypeError("values must be an object");/**
             * Enum values by id.
             * @type {Object.<number,string>}
             */ // toJSON
                // Note that values inherit valuesById on their prototype which makes them a TypeScript-
                // compatible enum. This is used by pbts to write actual enum definitions that work for
                // static and reflection code alike instead of emitting generic object definitions.
                if (this.valuesById = {}, this.values = Object.create(this.valuesById), this.comment = r, this.comments = s || {}, this.reserved = undefined$1, t) for (var a = Object.keys(t), d = 0; d < a.length; ++d)"number" == typeof t[a[d]] && (this.valuesById[this.values[a[d]] = t[a[d]]] = a[d])
            }/**
         * Enum descriptor.
         * @interface IEnum
         * @property {Object.<string,number>} values Enum values
         * @property {Object.<string,*>} [options] Enum options
         */ /**
         * Constructs an enum from an enum descriptor.
         * @param {string} name Enum name
         * @param {IEnum} json Enum descriptor
         * @returns {Enum} Created enum
         * @throws {TypeError} If arguments are invalid
         */t.exports = n;// extends ReflectionObject
            var o = e(24); ((n.prototype = Object.create(o.prototype)).constructor = n).className = "Enum"; var i = e(23), r = e(37);/**
         * Converts this enum to an enum descriptor.
         * @param {IToJSONOptions} [toJSONOptions] JSON conversion options
         * @returns {IEnum} Enum descriptor
         */ /**
         * Adds a value to this enum.
         * @param {string} name Value name
         * @param {number} id Value id
         * @param {string} [comment] Comment, if any
         * @returns {Enum} `this`
         * @throws {TypeError} If arguments are invalid
         * @throws {Error} If there is already a value with this name or id
         */ /**
         * Removes a value from this enum
         * @param {string} name Value name
         * @returns {Enum} `this`
         * @throws {TypeError} If arguments are invalid
         * @throws {Error} If `name` is not a name of this enum
         */ /**
         * Tests if the specified id is reserved.
         * @param {number} id Id to test
         * @returns {boolean} `true` if reserved, otherwise `false`
         */ /**
         * Tests if the specified name is reserved.
         * @param {string} name Name to test
         * @returns {boolean} `true` if reserved, otherwise `false`
         */n.fromJSON = function (e, t) { var o = new n(e, t.values, t.options, t.comment, t.comments); return o.reserved = t.reserved, o }, n.prototype.toJSON = function (e) { var t = !!e && !!e.keepComments; return r.toObject(["options", this.options, "values", this.values, "reserved", this.reserved && this.reserved.length ? this.reserved : undefined$1, "comment", t ? this.comment : undefined$1, "comments", t ? this.comments : undefined$1]) }, n.prototype.add = function (e, t, n) {// utilized by the parser but not by .fromJSON
                if (!r.isString(e)) throw TypeError("name must be a string"); if (!r.isInteger(t)) throw TypeError("id must be an integer"); if (this.values[e] !== undefined$1) throw Error("duplicate name '" + e + "' in " + this); if (this.isReservedId(t)) throw Error("id " + t + " is reserved in " + this); if (this.isReservedName(e)) throw Error("name '" + e + "' is reserved in " + this); if (this.valuesById[t] !== undefined$1) { if (!(this.options && this.options.allow_alias)) throw Error("duplicate id " + t + " in " + this); this.values[e] = t } else this.valuesById[this.values[e] = t] = e; return this.comments[e] = n || null, this
            }, n.prototype.remove = function (e) { if (!r.isString(e)) throw TypeError("name must be a string"); var t = this.values[e]; if (null == t) throw Error("name '" + e + "' does not exist in " + this); return delete this.valuesById[t], delete this.values[e], delete this.comments[e], this }, n.prototype.isReservedId = function (e) { return i.isReservedId(this.reserved, e) }, n.prototype.isReservedName = function (e) { return i.isReservedName(this.reserved, e) }
        }, { 23: 23, 24: 24, 37: 37 }], 16: [function (e, t) {/**
         * Not an actual constructor. Use {@link Field} instead.
         * @classdesc Base class of all reflected message fields. This is not an actual class but here for the sake of having consistent type definitions.
         * @exports FieldBase
         * @extends ReflectionObject
         * @constructor
         * @param {string} name Unique name within its namespace
         * @param {number} id Unique id within its namespace
         * @param {string} type Value type
         * @param {string|Object.<string,*>} [rule="optional"] Field rule
         * @param {string|Object.<string,*>} [extend] Extended type if different from parent
         * @param {Object.<string,*>} [options] Declared options
         * @param {string} [comment] Comment associated with this field
         */function n(e, t, n, i, d, p, l) {
                if (s.isObject(i) ? (l = d, p = i, i = d = undefined$1) : s.isObject(d) && (l = p, p = d, d = undefined$1), o.call(this, e, p), !s.isInteger(t) || 0 > t) throw TypeError("id must be a non-negative integer"); if (!s.isString(n)) throw TypeError("type must be a string"); if (i !== undefined$1 && !a.test(i = i.toString().toLowerCase())) throw TypeError("rule must be a string rule"); if (d !== undefined$1 && !s.isString(d)) throw TypeError("extend must be a string");/**
             * Field rule, if any.
             * @type {string|undefined}
             */this.rule = i && "optional" !== i ? i : undefined$1, this.type = n, this.id = t, this.extend = d || undefined$1, this.required = "required" === i, this.optional = !this.required, this.repeated = "repeated" === i, this.map = !1, this.message = null, this.partOf = null, this.typeDefault = null, this.defaultValue = null, this.long = !!s.Long && r.long[n] !== undefined$1, this.bytes = "bytes" === n, this.resolvedType = null, this.extensionField = null, this.declaringField = null, this._packed = null, this.comment = l
            }/**
         * Determines whether this field is packed. Only relevant when repeated and working with proto2.
         * @name Field#packed
         * @type {boolean}
         * @readonly
         */t.exports = n;// extends ReflectionObject
            var o = e(24); ((n.prototype = Object.create(o.prototype)).constructor = n).className = "Field"; var i = e(15), r = e(36), s = e(37), a = /^required|optional|repeated$/, d;/**
         * Constructs a new message field instance. Note that {@link MapField|map fields} have their own class.
         * @name Field
         * @classdesc Reflected message field.
         * @extends FieldBase
         * @constructor
         * @param {string} name Unique name within its namespace
         * @param {number} id Unique id within its namespace
         * @param {string} type Value type
         * @param {string|Object.<string,*>} [rule="optional"] Field rule
         * @param {string|Object.<string,*>} [extend] Extended type if different from parent
         * @param {Object.<string,*>} [options] Declared options
         */ /**
         * Constructs a field from a field descriptor.
         * @param {string} name Field name
         * @param {IField} json Field descriptor
         * @returns {Field} Created field
         * @throws {TypeError} If arguments are invalid
         */ /**
         * @override
         */ /**
         * Field descriptor.
         * @interface IField
         * @property {string} [rule="optional"] Field rule
         * @property {string} type Field type
         * @property {number} id Field id
         * @property {Object.<string,*>} [options] Field options
         */ /**
         * Extension field descriptor.
         * @interface IExtensionField
         * @extends IField
         * @property {string} extend Extended type
         */ /**
         * Converts this field to a field descriptor.
         * @param {IToJSONOptions} [toJSONOptions] JSON conversion options
         * @returns {IField} Field descriptor
         */ /**
         * Resolves this field's type references.
         * @returns {Field} `this`
         * @throws {Error} If any reference cannot be resolved
         */ /**
         * Decorator function as returned by {@link Field.d} and {@link MapField.d} (TypeScript).
         * @typedef FieldDecorator
         * @type {function}
         * @param {Object} prototype Target prototype
         * @param {string} fieldName Field name
         * @returns {undefined}
         */ /**
         * Field decorator (TypeScript).
         * @name Field.d
         * @function
         * @param {number} fieldId Field id
         * @param {"double"|"float"|"int32"|"uint32"|"sint32"|"fixed32"|"sfixed32"|"int64"|"uint64"|"sint64"|"fixed64"|"sfixed64"|"string"|"bool"|"bytes"|Object} fieldType Field type
         * @param {"optional"|"required"|"repeated"} [fieldRule="optional"] Field rule
         * @param {T} [defaultValue] Default value
         * @returns {FieldDecorator} Decorator function
         * @template T extends number | number[] | Long | Long[] | string | string[] | boolean | boolean[] | Uint8Array | Uint8Array[] | Buffer | Buffer[]
         */ /**
         * Field decorator (TypeScript).
         * @name Field.d
         * @function
         * @param {number} fieldId Field id
         * @param {Constructor<T>|string} fieldType Field type
         * @param {"optional"|"required"|"repeated"} [fieldRule="optional"] Field rule
         * @returns {FieldDecorator} Decorator function
         * @template T extends Message<T>
         * @variation 2
         */ // like Field.d but without a default value
            // Sets up cyclic dependencies (called in index-light)
            n.fromJSON = function (e, t) { return new n(e, t.id, t.type, t.rule, t.extend, t.options, t.comment) }, Object.defineProperty(n.prototype, "packed", { get: function () { return null === this._packed && (this._packed = !1 !== this.getOption("packed")), this._packed } }), n.prototype.setOption = function (e, t, n) {
                return "packed" === e && (// clear cached before setting
                    this._packed = null), o.prototype.setOption.call(this, e, t, n)
            }, n.prototype.toJSON = function (e) { var t = !!e && !!e.keepComments; return s.toObject(["rule", "optional" !== this.rule && this.rule || undefined$1, "type", this.type, "id", this.id, "extend", this.extend, "options", this.options, "comment", t ? this.comment : undefined$1]) }, n.prototype.resolve = function () {
                if (this.resolved) return this;// convert to internal data type if necesssary
                if ((this.typeDefault = r.defaults[this.type]) === undefined$1 && (this.resolvedType = (this.declaringField ? this.declaringField.parent : this.parent).lookupTypeOrEnum(this.type), this.typeDefault = this.resolvedType instanceof d ? null :// instanceof Enum
                    this.resolvedType.values[Object.keys(this.resolvedType.values)[0]]), this.options && null != this.options["default"] && (this.typeDefault = this.options["default"], this.resolvedType instanceof i && "string" == typeof this.typeDefault && (this.typeDefault = this.resolvedType.values[this.typeDefault])), this.options && ((!0 === this.options.packed || this.options.packed !== undefined$1 && this.resolvedType && !(this.resolvedType instanceof i)) && delete this.options.packed, !Object.keys(this.options).length && (this.options = undefined$1)), this.long) this.typeDefault = s.Long.fromNumber(this.typeDefault, "u" === this.type.charAt(0)), Object.freeze && Object.freeze(this.typeDefault); else if (this.bytes && "string" == typeof this.typeDefault) { var e; s.base64.test(this.typeDefault) ? s.base64.decode(this.typeDefault, e = s.newBuffer(s.base64.length(this.typeDefault)), 0) : s.utf8.write(this.typeDefault, e = s.newBuffer(s.utf8.length(this.typeDefault)), 0), this.typeDefault = e }// take special care of maps and repeated fields
                return this.defaultValue = this.map ? s.emptyObject : this.repeated ? s.emptyArray : this.typeDefault, this.parent instanceof d && (this.parent.ctor.prototype[this.name] = this.defaultValue), o.prototype.resolve.call(this)
            }, n.d = function (e, t, o, i) { return "function" == typeof t ? t = s.decorateType(t).name : t && "object" == typeof t && (t = s.decorateEnum(t).name), function (r, a) { s.decorateType(r.constructor).add(new n(a, e, t, o, { default: i })) } }, n._configure = function (e) { d = e }
        }, { 15: 15, 24: 24, 36: 36, 37: 37 }], 17: [function (e, t) {/**
         * A node-style callback as used by {@link load} and {@link Root#load}.
         * @typedef LoadCallback
         * @type {function}
         * @param {Error|null} error Error, if any, otherwise `null`
         * @param {Root} [root] Root, if there hasn't been an error
         * @returns {undefined}
         */ /**
         * Loads one or multiple .proto or preprocessed .json files into a common root namespace and calls the callback.
         * @param {string|string[]} filename One or multiple files to load
         * @param {Root} root Root namespace, defaults to create a new one if omitted.
         * @param {LoadCallback} callback Callback function
         * @returns {undefined}
         * @see {@link Root#load}
         */ /**
         * Synchronously loads one or multiple .proto or preprocessed .json files into a common root namespace (node only).
         * @param {string|string[]} filename One or multiple files to load
         * @param {Root} [root] Root namespace, defaults to create a new one if omitted.
         * @returns {Root} Root namespace
         * @throws {Error} If synchronous fetching is not supported (i.e. in browsers) or if a file's syntax is invalid
         * @see {@link Root#loadSync}
         */var n = t.exports = e(18);/**
         * Loads one or multiple .proto or preprocessed .json files into a common root namespace and calls the callback.
         * @name load
         * @function
         * @param {string|string[]} filename One or multiple files to load
         * @param {LoadCallback} callback Callback function
         * @returns {undefined}
         * @see {@link Root#load}
         * @variation 2
         */ // function load(filename:string, callback:LoadCallback):undefined
            /**
                     * Loads one or multiple .proto or preprocessed .json files into a common root namespace and returns a promise.
                     * @name load
                     * @function
                     * @param {string|string[]} filename One or multiple files to load
                     * @param {Root} [root] Root namespace, defaults to create a new one if omitted.
                     * @returns {Promise<Root>} Promise
                     * @see {@link Root#load}
                     * @variation 3
                     */ // function load(filename:string, [root:Root]):Promise<Root>
            // Serialization
            // Reflection
            // Runtime
            // Utility
            // Set up possibly cyclic reflection dependencies
            n.build = "light", n.load = function (e, t, o) { return "function" == typeof t ? (o = t, t = new n.Root) : !t && (t = new n.Root), t.load(e, o) }, n.loadSync = function (e, t) { return t || (t = new n.Root), t.loadSync(e) }, n.encoder = e(14), n.decoder = e(13), n.verifier = e(40), n.converter = e(12), n.ReflectionObject = e(24), n.Namespace = e(23), n.Root = e(29), n.Enum = e(15), n.Type = e(35), n.Field = e(16), n.OneOf = e(25), n.MapField = e(20), n.Service = e(33), n.Method = e(22), n.Message = e(21), n.wrappers = e(41), n.types = e(36), n.util = e(37), n.ReflectionObject._configure(n.Root), n.Namespace._configure(n.Type, n.Service, n.Enum), n.Root._configure(n.Type), n.Field._configure(n.Type)
        }, { 12: 12, 13: 13, 14: 14, 15: 15, 16: 16, 18: 18, 20: 20, 21: 21, 22: 22, 23: 23, 24: 24, 25: 25, 29: 29, 33: 33, 35: 35, 36: 36, 37: 37, 40: 40, 41: 41 }], 18: [function (e, t, n) {/* istanbul ignore next */ /**
         * Reconfigures the library according to the environment.
         * @returns {undefined}
         */function o() { i.Reader._configure(i.BufferReader), i.util._configure() }// Set up buffer utility according to the environment
            var i = n;/**
         * Build type, one of `"full"`, `"light"` or `"minimal"`.
         * @name build
         * @type {string}
         * @const
         */ // Serialization
            // Utility
            i.build = "minimal", i.Writer = e(42), i.BufferWriter = e(43), i.Reader = e(27), i.BufferReader = e(28), i.util = e(39), i.rpc = e(31), i.roots = e(30), i.configure = o, i.Writer._configure(i.BufferWriter), o()
        }, { 27: 27, 28: 28, 30: 30, 31: 31, 39: 39, 42: 42, 43: 43 }], 19: [function (e, t) {
            var n = t.exports = e(17);// Parser
            // Configure parser
            n.build = "full", n.tokenize = e(34), n.parse = e(26), n.common = e(11), n.Root._configure(n.Type, n.parse, n.common)
        }, { 11: 11, 17: 17, 26: 26, 34: 34 }], 20: [function (e, t) {/**
         * Constructs a new map field instance.
         * @classdesc Reflected map field.
         * @extends FieldBase
         * @constructor
         * @param {string} name Unique name within its namespace
         * @param {number} id Unique id within its namespace
         * @param {string} keyType Key type
         * @param {string} type Value type
         * @param {Object.<string,*>} [options] Declared options
         * @param {string} [comment] Comment associated with this field
         */function n(e, t, n, i, s, a) {/* istanbul ignore if */if (o.call(this, e, t, i, undefined$1, undefined$1, s, a), !r.isString(n)) throw TypeError("keyType must be a string");/**
             * Key type.
             * @type {string}
             */this.keyType = n, this.resolvedKeyType = null, this.map = !0
            }/**
         * Map field descriptor.
         * @interface IMapField
         * @extends {IField}
         * @property {string} keyType Key type
         */ /**
         * Extension map field descriptor.
         * @interface IExtensionMapField
         * @extends IMapField
         * @property {string} extend Extended type
         */ /**
         * Constructs a map field from a map field descriptor.
         * @param {string} name Field name
         * @param {IMapField} json Map field descriptor
         * @returns {MapField} Created map field
         * @throws {TypeError} If arguments are invalid
         */t.exports = n;// extends Field
            var o = e(16); ((n.prototype = Object.create(o.prototype)).constructor = n).className = "MapField"; var i = e(36), r = e(37);/**
         * Converts this map field to a map field descriptor.
         * @param {IToJSONOptions} [toJSONOptions] JSON conversion options
         * @returns {IMapField} Map field descriptor
         */ /**
         * @override
         */ /**
         * Map field decorator (TypeScript).
         * @name MapField.d
         * @function
         * @param {number} fieldId Field id
         * @param {"int32"|"uint32"|"sint32"|"fixed32"|"sfixed32"|"int64"|"uint64"|"sint64"|"fixed64"|"sfixed64"|"bool"|"string"} fieldKeyType Field key type
         * @param {"double"|"float"|"int32"|"uint32"|"sint32"|"fixed32"|"sfixed32"|"int64"|"uint64"|"sint64"|"fixed64"|"sfixed64"|"bool"|"string"|"bytes"|Object|Constructor<{}>} fieldValueType Field value type
         * @returns {FieldDecorator} Decorator function
         * @template T extends { [key: string]: number | Long | string | boolean | Uint8Array | Buffer | number[] | Message<{}> }
         */n.fromJSON = function (e, t) { return new n(e, t.id, t.keyType, t.type, t.options, t.comment) }, n.prototype.toJSON = function (e) { var t = !!e && !!e.keepComments; return r.toObject(["keyType", this.keyType, "type", this.type, "id", this.id, "extend", this.extend, "options", this.options, "comment", t ? this.comment : undefined$1]) }, n.prototype.resolve = function () {
                if (this.resolved) return this;// Besides a value type, map fields have a key type that may be "any scalar type except for floating point types and bytes"
                if (i.mapKey[this.keyType] === undefined$1) throw Error("invalid key type: " + this.keyType); return o.prototype.resolve.call(this)
            }, n.d = function (e, t, o) { return "function" == typeof o ? o = r.decorateType(o).name : o && "object" == typeof o && (o = r.decorateEnum(o).name), function (i, s) { r.decorateType(i.constructor).add(new n(s, e, t, o)) } }
        }, { 16: 16, 36: 36, 37: 37 }], 21: [function (e, t) {/**
         * Constructs a new message instance.
         * @classdesc Abstract runtime message.
         * @constructor
         * @param {Properties<T>} [properties] Properties to set
         * @template T extends object = object
         */function n(e) {// not used internally
                if (e) for (var t = Object.keys(e), n = 0; n < t.length; ++n)this[t[n]] = e[t[n]]
            }/**
         * Reference to the reflected type.
         * @name Message.$type
         * @type {Type}
         * @readonly
         */ /**
         * Reference to the reflected type.
         * @name Message#$type
         * @type {Type}
         * @readonly
         */ /*eslint-disable valid-jsdoc*/ /**
         * Creates a new message of this type using the specified properties.
         * @param {Object.<string,*>} [properties] Properties to set
         * @returns {Message<T>} Message instance
         * @template T extends Message<T>
         * @this Constructor<T>
         */t.exports = n; var o = e(39);/**
         * Encodes a message of this type.
         * @param {T|Object.<string,*>} message Message to encode
         * @param {Writer} [writer] Writer to use
         * @returns {Writer} Writer
         * @template T extends Message<T>
         * @this Constructor<T>
         */ /**
         * Encodes a message of this type preceeded by its length as a varint.
         * @param {T|Object.<string,*>} message Message to encode
         * @param {Writer} [writer] Writer to use
         * @returns {Writer} Writer
         * @template T extends Message<T>
         * @this Constructor<T>
         */ /**
         * Decodes a message of this type.
         * @name Message.decode
         * @function
         * @param {Reader|Uint8Array} reader Reader or buffer to decode
         * @returns {T} Decoded message
         * @template T extends Message<T>
         * @this Constructor<T>
         */ /**
         * Decodes a message of this type preceeded by its length as a varint.
         * @name Message.decodeDelimited
         * @function
         * @param {Reader|Uint8Array} reader Reader or buffer to decode
         * @returns {T} Decoded message
         * @template T extends Message<T>
         * @this Constructor<T>
         */ /**
         * Verifies a message of this type.
         * @name Message.verify
         * @function
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */ /**
         * Creates a new message of this type from a plain object. Also converts values to their respective internal types.
         * @param {Object.<string,*>} object Plain object
         * @returns {T} Message instance
         * @template T extends Message<T>
         * @this Constructor<T>
         */ /**
         * Creates a plain object from a message of this type. Also converts values to other types if specified.
         * @param {T} message Message instance
         * @param {IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         * @template T extends Message<T>
         * @this Constructor<T>
         */ /**
         * Converts this message to JSON.
         * @returns {Object.<string,*>} JSON object
         */n.create = function (e) { return this.$type.create(e) }, n.encode = function (e, t) { return this.$type.encode(e, t) }, n.encodeDelimited = function (e, t) { return this.$type.encodeDelimited(e, t) }, n.decode = function (e) { return this.$type.decode(e) }, n.decodeDelimited = function (e) { return this.$type.decodeDelimited(e) }, n.verify = function (e) { return this.$type.verify(e) }, n.fromObject = function (e) { return this.$type.fromObject(e) }, n.toObject = function (e, t) { return this.$type.toObject(e, t) }, n.prototype.toJSON = function () { return this.$type.toObject(this, o.toJSONOptions) }
        }, { 39: 39 }], 22: [function (e, t) {/**
         * Constructs a new service method instance.
         * @classdesc Reflected service method.
         * @extends ReflectionObject
         * @constructor
         * @param {string} name Method name
         * @param {string|undefined} type Method type, usually `"rpc"`
         * @param {string} requestType Request message type
         * @param {string} responseType Response message type
         * @param {boolean|Object.<string,*>} [requestStream] Whether the request is streamed
         * @param {boolean|Object.<string,*>} [responseStream] Whether the response is streamed
         * @param {Object.<string,*>} [options] Declared options
         * @param {string} [comment] The comment for this method
         */function n(e, t, n, r, s, a, d, p) {/* istanbul ignore if */if (i.isObject(s) ? (d = s, s = a = undefined$1) : i.isObject(a) && (d = a, a = undefined$1), !(t === undefined$1 || i.isString(t))) throw TypeError("type must be a string");/* istanbul ignore if */if (!i.isString(n)) throw TypeError("requestType must be a string");/* istanbul ignore if */if (!i.isString(r)) throw TypeError("responseType must be a string"); o.call(this, e, d), this.type = t || "rpc", this.requestType = n, this.requestStream = !!s || undefined$1, this.responseType = r, this.responseStream = !!a || undefined$1, this.resolvedRequestType = null, this.resolvedResponseType = null, this.comment = p }/**
         * Method descriptor.
         * @interface IMethod
         * @property {string} [type="rpc"] Method type
         * @property {string} requestType Request type
         * @property {string} responseType Response type
         * @property {boolean} [requestStream=false] Whether requests are streamed
         * @property {boolean} [responseStream=false] Whether responses are streamed
         * @property {Object.<string,*>} [options] Method options
         */ /**
         * Constructs a method from a method descriptor.
         * @param {string} name Method name
         * @param {IMethod} json Method descriptor
         * @returns {Method} Created method
         * @throws {TypeError} If arguments are invalid
         */t.exports = n;// extends ReflectionObject
            var o = e(24); ((n.prototype = Object.create(o.prototype)).constructor = n).className = "Method"; var i = e(37);/**
         * Converts this method to a method descriptor.
         * @param {IToJSONOptions} [toJSONOptions] JSON conversion options
         * @returns {IMethod} Method descriptor
         */ /**
         * @override
         */n.fromJSON = function (e, t) { return new n(e, t.type, t.requestType, t.responseType, t.requestStream, t.responseStream, t.options, t.comment) }, n.prototype.toJSON = function (e) { var t = !!e && !!e.keepComments; return i.toObject(["type", "rpc" !== this.type &&/* istanbul ignore next */this.type || undefined$1, "requestType", this.requestType, "requestStream", this.requestStream, "responseType", this.responseType, "responseStream", this.responseStream, "options", this.options, "comment", t ? this.comment : undefined$1]) }, n.prototype.resolve = function () {/* istanbul ignore if */return this.resolved ? this : (this.resolvedRequestType = this.parent.lookupType(this.requestType), this.resolvedResponseType = this.parent.lookupType(this.responseType), o.prototype.resolve.call(this)) }
        }, { 24: 24, 37: 37 }], 23: [function (e, t) {/**
         * Converts an array of reflection objects to JSON.
         * @memberof Namespace
         * @param {ReflectionObject[]} array Object array
         * @param {IToJSONOptions} [toJSONOptions] JSON conversion options
         * @returns {Object.<string,*>|undefined} JSON object or `undefined` when array is empty
         */function n(e, t) { if (!(e && e.length)) return undefined$1; for (var n = {}, o = 0; o < e.length; ++o)n[e[o].name] = e[o].toJSON(t); return n }/**
         * Not an actual constructor. Use {@link Namespace} instead.
         * @classdesc Base class of all reflection objects containing nested objects. This is not an actual class but here for the sake of having consistent type definitions.
         * @exports NamespaceBase
         * @extends ReflectionObject
         * @abstract
         * @constructor
         * @param {string} name Namespace name
         * @param {Object.<string,*>} [options] Declared options
         * @see {@link Namespace}
         */function o(e, t) {/**
             * Nested objects by name.
             * @type {Object.<string,ReflectionObject>|undefined}
             */ // toJSON
/**
             * Cached nested objects as an array.
             * @type {ReflectionObject[]|null}
             * @private
             */s.call(this, e, t), this.nested = undefined$1, this._nestedArray = null
            } function r(e) { return e._nestedArray = null, e }/**
         * Nested objects of this namespace as an array for iteration.
         * @name NamespaceBase#nestedArray
         * @type {ReflectionObject[]}
         * @readonly
         */t.exports = o;// extends ReflectionObject
            var s = e(24); ((o.prototype = Object.create(s.prototype)).constructor = o).className = "Namespace"; var a = e(16), d = e(37), p,// cyclic
                l, u;/**
         * Constructs a new namespace instance.
         * @name Namespace
         * @classdesc Reflected namespace.
         * @extends NamespaceBase
         * @constructor
         * @param {string} name Namespace name
         * @param {Object.<string,*>} [options] Declared options
         */ /**
         * Constructs a namespace from JSON.
         * @memberof Namespace
         * @function
         * @param {string} name Namespace name
         * @param {Object.<string,*>} json JSON object
         * @returns {Namespace} Created namespace
         * @throws {TypeError} If arguments are invalid
         */ /**
         * Tests if the specified id is reserved.
         * @param {Array.<number[]|string>|undefined} reserved Array of reserved ranges and names
         * @param {number} id Id to test
         * @returns {boolean} `true` if reserved, otherwise `false`
         */ /**
         * Tests if the specified name is reserved.
         * @param {Array.<number[]|string>|undefined} reserved Array of reserved ranges and names
         * @param {string} name Name to test
         * @returns {boolean} `true` if reserved, otherwise `false`
         */ /**
         * Namespace descriptor.
         * @interface INamespace
         * @property {Object.<string,*>} [options] Namespace options
         * @property {Object.<string,AnyNestedObject>} [nested] Nested object descriptors
         */ /**
         * Any extension field descriptor.
         * @typedef AnyExtensionField
         * @type {IExtensionField|IExtensionMapField}
         */ /**
         * Any nested object descriptor.
         * @typedef AnyNestedObject
         * @type {IEnum|IType|IService|AnyExtensionField|INamespace}
         */ // ^ BEWARE: VSCode hangs forever when using more than 5 types (that's why AnyExtensionField exists in the first place)
            /**
                     * Converts this namespace to a namespace descriptor.
                     * @param {IToJSONOptions} [toJSONOptions] JSON conversion options
                     * @returns {INamespace} Namespace descriptor
                     */ /**
            * Adds nested objects to this namespace from nested object descriptors.
            * @param {Object.<string,AnyNestedObject>} nestedJson Any nested object descriptors
            * @returns {Namespace} `this`
            */ /**
            * Gets the nested object of the specified name.
            * @param {string} name Nested object name
            * @returns {ReflectionObject|null} The reflection object or `null` if it doesn't exist
            */ /**
            * Gets the values of the nested {@link Enum|enum} of the specified name.
            * This methods differs from {@link Namespace#get|get} in that it returns an enum's values directly and throws instead of returning `null`.
            * @param {string} name Nested enum name
            * @returns {Object.<string,number>} Enum values
            * @throws {Error} If there is no such enum
            */ /**
            * Adds a nested object to this namespace.
            * @param {ReflectionObject} object Nested object to add
            * @returns {Namespace} `this`
            * @throws {TypeError} If arguments are invalid
            * @throws {Error} If there is already a nested object with this name
            */ /**
            * Removes a nested object from this namespace.
            * @param {ReflectionObject} object Nested object to remove
            * @returns {Namespace} `this`
            * @throws {TypeError} If arguments are invalid
            * @throws {Error} If `object` is not a member of this namespace
            */ /**
            * Defines additial namespaces within this one if not yet existing.
            * @param {string|string[]} path Path to create
            * @param {*} [json] Nested types to create from JSON
            * @returns {Namespace} Pointer to the last namespace created or `this` if path is empty
            */ /**
            * Resolves this namespace's and all its nested objects' type references. Useful to validate a reflection tree, but comes at a cost.
            * @returns {Namespace} `this`
            */ /**
            * Recursively looks up the reflection object matching the specified path in the scope of this namespace.
            * @param {string|string[]} path Path to look up
            * @param {*|Array.<*>} filterTypes Filter types, any combination of the constructors of `protobuf.Type`, `protobuf.Enum`, `protobuf.Service` etc.
            * @param {boolean} [parentAlreadyChecked=false] If known, whether the parent has already been checked
            * @returns {ReflectionObject|null} Looked up object or `null` if none could be found
            */ /**
            * Looks up the reflection object at the specified path, relative to this namespace.
            * @name NamespaceBase#lookup
            * @function
            * @param {string|string[]} path Path to look up
            * @param {boolean} [parentAlreadyChecked=false] Whether the parent has already been checked
            * @returns {ReflectionObject|null} Looked up object or `null` if none could be found
            * @variation 2
            */ // lookup(path: string, [parentAlreadyChecked: boolean])
            /**
                     * Looks up the {@link Type|type} at the specified path, relative to this namespace.
                     * Besides its signature, this methods differs from {@link Namespace#lookup|lookup} in that it throws instead of returning `null`.
                     * @param {string|string[]} path Path to look up
                     * @returns {Type} Looked up type
                     * @throws {Error} If `path` does not point to a type
                     */ /**
            * Looks up the values of the {@link Enum|enum} at the specified path, relative to this namespace.
            * Besides its signature, this methods differs from {@link Namespace#lookup|lookup} in that it throws instead of returning `null`.
            * @param {string|string[]} path Path to look up
            * @returns {Enum} Looked up enum
            * @throws {Error} If `path` does not point to an enum
            */ /**
            * Looks up the {@link Type|type} or {@link Enum|enum} at the specified path, relative to this namespace.
            * Besides its signature, this methods differs from {@link Namespace#lookup|lookup} in that it throws instead of returning `null`.
            * @param {string|string[]} path Path to look up
            * @returns {Type} Looked up type or enum
            * @throws {Error} If `path` does not point to a type or enum
            */ /**
            * Looks up the {@link Service|service} at the specified path, relative to this namespace.
            * Besides its signature, this methods differs from {@link Namespace#lookup|lookup} in that it throws instead of returning `null`.
            * @param {string|string[]} path Path to look up
            * @returns {Service} Looked up service
            * @throws {Error} If `path` does not point to a service
            */ // Sets up cyclic dependencies (called in index-light)
            o.fromJSON = function (e, t) { return new o(e, t.options).addJSON(t.nested) }, o.arrayToJSON = n, o.isReservedId = function (e, t) { if (e) for (var n = 0; n < e.length; ++n)if ("string" != typeof e[n] && e[n][0] <= t && e[n][1] >= t) return !0; return !1 }, o.isReservedName = function (e, t) { if (e) for (var n = 0; n < e.length; ++n)if (e[n] === t) return !0; return !1 }, Object.defineProperty(o.prototype, "nestedArray", { get: function () { return this._nestedArray || (this._nestedArray = d.toArray(this.nested)) } }), o.prototype.toJSON = function (e) { return d.toObject(["options", this.options, "nested", n(this.nestedArray, e)]) }, o.prototype.addJSON = function (e) {
                var t = this;/* istanbul ignore else */if (e) for (var n = Object.keys(e), r = 0, s; r < n.length; ++r)s = e[n[r]], t.add(// most to least likely
                    (s.fields === undefined$1 ? s.values === undefined$1 ? s.methods === undefined$1 ? s.id === undefined$1 ? o.fromJSON : a.fromJSON : l.fromJSON : u.fromJSON : p.fromJSON)(n[r], s)); return this
            }, o.prototype.get = function (e) { return this.nested && this.nested[e] || null }, o.prototype.getEnum = function (e) { if (this.nested && this.nested[e] instanceof u) return this.nested[e].values; throw Error("no such enum: " + e) }, o.prototype.add = function (e) {
                if (!(e instanceof a && e.extend !== undefined$1 || e instanceof p || e instanceof u || e instanceof l || e instanceof o)) throw TypeError("object must be a valid nested object"); if (!this.nested) this.nested = {}; else {
                    var t = this.get(e.name); if (t) if (t instanceof o && e instanceof o && !(t instanceof p || t instanceof l)) {// replace plain namespace but keep existing nested elements and options
                        for (var n = t.nestedArray, s = 0; s < n.length; ++s)e.add(n[s]); this.remove(t), this.nested || (this.nested = {}), e.setOptions(t.options, !0)
                    } else throw Error("duplicate name '" + e.name + "' in " + this)
                } return this.nested[e.name] = e, e.onAdd(this), r(this)
            }, o.prototype.remove = function (e) { if (!(e instanceof s)) throw TypeError("object must be a ReflectionObject"); if (e.parent !== this) throw Error(e + " is not a member of " + this); return delete this.nested[e.name], Object.keys(this.nested).length || (this.nested = undefined$1), e.onRemove(this), r(this) }, o.prototype.define = function (e, t) { if (d.isString(e)) e = e.split("."); else if (!Array.isArray(e)) throw TypeError("illegal path"); if (e && e.length && "" === e[0]) throw Error("path must be relative"); for (var n = this, i; 0 < e.length;)if (i = e.shift(), !(n.nested && n.nested[i])) n.add(n = new o(i)); else if (n = n.nested[i], !(n instanceof o)) throw Error("path conflicts with non-namespace objects"); return t && n.addJSON(t), n }, o.prototype.resolveAll = function () { for (var e = this.nestedArray, t = 0; t < e.length;)e[t] instanceof o ? e[t++].resolveAll() : e[t++].resolve(); return this.resolve() }, o.prototype.lookup = function (e, t, n) {
                if ("boolean" == typeof t ? (n = t, t = undefined$1) : t && !Array.isArray(t) && (t = [t]), d.isString(e) && e.length) { if ("." === e) return this.root; e = e.split(".") } else if (!e.length) return this;// Start at root if path is absolute
                if ("" === e[0]) return this.root.lookup(e.slice(1), t);// Test if the first part matches any nested object, and if so, traverse if path contains more
                var r = this.get(e[0]); if (!r) { for (var s = 0; s < this.nestedArray.length; ++s)if (this._nestedArray[s] instanceof o && (r = this._nestedArray[s].lookup(e, t, !0))) return r; } else if (1 === e.length) { if (!t || -1 < t.indexOf(r.constructor)) return r; } else if (r instanceof o && (r = r.lookup(e.slice(1), t, !0))) return r;// Otherwise try each nested namespace
                // If there hasn't been a match, try again at the parent
                return null === this.parent || n ? null : this.parent.lookup(e, t)
            }, o.prototype.lookupType = function (e) { var t = this.lookup(e, [p]); if (!t) throw Error("no such type: " + e); return t }, o.prototype.lookupEnum = function (e) { var t = this.lookup(e, [u]); if (!t) throw Error("no such Enum '" + e + "' in " + this); return t }, o.prototype.lookupTypeOrEnum = function (e) { var t = this.lookup(e, [p, u]); if (!t) throw Error("no such Type or Enum '" + e + "' in " + this); return t }, o.prototype.lookupService = function (e) { var t = this.lookup(e, [l]); if (!t) throw Error("no such Service '" + e + "' in " + this); return t }, o._configure = function (e, t, n) { p = e, l = t, u = n }
        }, { 16: 16, 24: 24, 37: 37 }], 24: [function (e, t) {// cyclic
/**
         * Constructs a new reflection object instance.
         * @classdesc Base class of all reflection objects.
         * @constructor
         * @param {string} name Object name
         * @param {Object.<string,*>} [options] Declared options
         * @abstract
         */function n(e, t) {
                if (!o.isString(e)) throw TypeError("name must be a string"); if (t && !o.isObject(t)) throw TypeError("options must be an object");/**
             * Options.
             * @type {Object.<string,*>|undefined}
             */ // toJSON
/**
             * Unique name within its namespace.
             * @type {string}
             */ /**
             * Parent namespace.
             * @type {Namespace|null}
             */ /**
             * Whether already resolved or not.
             * @type {boolean}
             */ /**
             * Comment text, if any.
             * @type {string|null}
             */ /**
             * Defining file name.
             * @type {string|null}
             */this.options = t, this.name = e, this.parent = null, this.resolved = !1, this.comment = null, this.filename = null
            } t.exports = n, n.className = "ReflectionObject"; var o = e(37), i;/**
         * Converts this reflection object to its descriptor representation.
         * @returns {Object.<string,*>} Descriptor
         * @abstract
         */ /**
         * Called when this object is added to a parent.
         * @param {ReflectionObject} parent Parent added to
         * @returns {undefined}
         */ /**
         * Called when this object is removed from a parent.
         * @param {ReflectionObject} parent Parent removed from
         * @returns {undefined}
         */ /**
         * Resolves this objects type references.
         * @returns {ReflectionObject} `this`
         */ /**
         * Gets an option value.
         * @param {string} name Option name
         * @returns {*} Option value or `undefined` if not set
         */ /**
         * Sets an option.
         * @param {string} name Option name
         * @param {*} value Option value
         * @param {boolean} [ifNotSet] Sets the option only if it isn't currently set
         * @returns {ReflectionObject} `this`
         */ /**
         * Sets multiple options.
         * @param {Object.<string,*>} options Options to set
         * @param {boolean} [ifNotSet] Sets an option only if it isn't currently set
         * @returns {ReflectionObject} `this`
         */ /**
         * Converts this instance to its string representation.
         * @returns {string} Class name[, space, full name]
         */ // Sets up cyclic dependencies (called in index-light)
            Object.defineProperties(n.prototype, {/**
             * Reference to the root namespace.
             * @name ReflectionObject#root
             * @type {Root}
             * @readonly
             */root: { get: function () { for (var e = this; null !== e.parent;)e = e.parent; return e } },/**
             * Full name including leading dot.
             * @name ReflectionObject#fullName
             * @type {string}
             * @readonly
             */fullName: { get: function () { for (var e = [this.name], t = this.parent; t;)e.unshift(t.name), t = t.parent; return e.join(".") } }
            }), n.prototype.toJSON =/* istanbul ignore next */function () {
                throw Error();// not implemented, shouldn't happen
            }, n.prototype.onAdd = function (e) { this.parent && this.parent !== e && this.parent.remove(this), this.parent = e, this.resolved = !1; var t = e.root; t instanceof i && t._handleAdd(this) }, n.prototype.onRemove = function (e) { var t = e.root; t instanceof i && t._handleRemove(this), this.parent = null, this.resolved = !1 }, n.prototype.resolve = function () { return this.resolved ? this : (this.root instanceof i && (this.resolved = !0), this) }, n.prototype.getOption = function (e) { return this.options ? this.options[e] : undefined$1 }, n.prototype.setOption = function (e, t, n) { return n && this.options && this.options[e] !== undefined$1 || ((this.options || (this.options = {}))[e] = t), this }, n.prototype.setOptions = function (e, t) { if (e) for (var n = Object.keys(e), o = 0; o < n.length; ++o)this.setOption(n[o], e[n[o]], t); return this }, n.prototype.toString = function () { var e = this.constructor.className, t = this.fullName; return t.length ? e + " " + t : e }, n._configure = function (e) { i = e }
        }, { 37: 37 }], 25: [function (e, t) {/**
         * Constructs a new oneof instance.
         * @classdesc Reflected oneof.
         * @extends ReflectionObject
         * @constructor
         * @param {string} name Oneof name
         * @param {string[]|Object.<string,*>} [fieldNames] Field names
         * @param {Object.<string,*>} [options] Declared options
         * @param {string} [comment] Comment associated with this field
         */function n(e, t, n, o) {/* istanbul ignore if */if (Array.isArray(t) || (n = t, t = undefined$1), r.call(this, e, n), !(t === undefined$1 || Array.isArray(t))) throw TypeError("fieldNames must be an Array");/**
             * Field names that belong to this oneof.
             * @type {string[]}
             */this.oneof = t || [], this.fieldsArray = [], this.comment = o
            }/**
         * Oneof descriptor.
         * @interface IOneOf
         * @property {Array.<string>} oneof Oneof field names
         * @property {Object.<string,*>} [options] Oneof options
         */ /**
         * Constructs a oneof from a oneof descriptor.
         * @param {string} name Oneof name
         * @param {IOneOf} json Oneof descriptor
         * @returns {OneOf} Created oneof
         * @throws {TypeError} If arguments are invalid
         */ /**
         * Adds the fields of the specified oneof to the parent if not already done so.
         * @param {OneOf} oneof The oneof
         * @returns {undefined}
         * @inner
         * @ignore
         */function o(e) { if (e.parent) for (var t = 0; t < e.fieldsArray.length; ++t)e.fieldsArray[t].parent || e.parent.add(e.fieldsArray[t]) }/**
         * Adds a field to this oneof and removes it from its current parent, if any.
         * @param {Field} field Field to add
         * @returns {OneOf} `this`
         */t.exports = n;// extends ReflectionObject
            var r = e(24); ((n.prototype = Object.create(r.prototype)).constructor = n).className = "OneOf"; var s = e(16), a = e(37);/**
         * Converts this oneof to a oneof descriptor.
         * @param {IToJSONOptions} [toJSONOptions] JSON conversion options
         * @returns {IOneOf} Oneof descriptor
         */ /**
         * Removes a field from this oneof and puts it back to the oneof's parent.
         * @param {Field} field Field to remove
         * @returns {OneOf} `this`
         */ /**
         * @override
         */ /**
         * @override
         */ /**
         * Decorator function as returned by {@link OneOf.d} (TypeScript).
         * @typedef OneOfDecorator
         * @type {function}
         * @param {Object} prototype Target prototype
         * @param {string} oneofName OneOf name
         * @returns {undefined}
         */ /**
         * OneOf decorator (TypeScript).
         * @function
         * @param {...string} fieldNames Field names
         * @returns {OneOfDecorator} Decorator function
         * @template T extends string
         */n.fromJSON = function (e, t) { return new n(e, t.oneof, t.options, t.comment) }, n.prototype.toJSON = function (e) { var t = !!e && !!e.keepComments; return a.toObject(["options", this.options, "oneof", this.oneof, "comment", t ? this.comment : undefined$1]) }, n.prototype.add = function (e) {/* istanbul ignore if */if (!(e instanceof s)) throw TypeError("field must be a Field"); return e.parent && e.parent !== this.parent && e.parent.remove(e), this.oneof.push(e.name), this.fieldsArray.push(e), e.partOf = this, o(this), this }, n.prototype.remove = function (e) {/* istanbul ignore if */if (!(e instanceof s)) throw TypeError("field must be a Field"); var t = this.fieldsArray.indexOf(e);/* istanbul ignore if */if (0 > t) throw Error(e + " is not a member of " + this); return this.fieldsArray.splice(t, 1), t = this.oneof.indexOf(e.name), -1 < t &&// theoretical
                this.oneof.splice(t, 1), e.partOf = null, this
            }, n.prototype.onAdd = function (e) {
                r.prototype.onAdd.call(this, e);// Collect present fields
                for (var t = this, n = 0, s; n < this.oneof.length; ++n)s = e.get(this.oneof[n]), s && !s.partOf && (s.partOf = t, t.fieldsArray.push(s));// Add not yet present fields
                o(this)
            }, n.prototype.onRemove = function (e) { for (var t = 0, n; t < this.fieldsArray.length; ++t)(n = this.fieldsArray[t]).parent && n.parent.remove(n); r.prototype.onRemove.call(this, e) }, n.d = function () { for (var e = Array(arguments.length), t = 0; t < arguments.length;)e[t] = arguments[t++]; return function (t, o) { a.decorateType(t.constructor).add(new n(o, e)), Object.defineProperty(t, o, { get: a.oneOfGetter(e), set: a.oneOfSetter(e) }) } }
        }, { 16: 16, 24: 24, 37: 37 }], 26: [function (e, t) {/**
         * Result object returned from {@link parse}.
         * @interface IParserResult
         * @property {string|undefined} package Package name, if declared
         * @property {string[]|undefined} imports Imports, if any
         * @property {string[]|undefined} weakImports Weak imports, if any
         * @property {string|undefined} syntax Syntax, if specified (either `"proto2"` or `"proto3"`)
         * @property {Root} root Populated root instance
         */ /**
         * Options modifying the behavior of {@link parse}.
         * @interface IParseOptions
         * @property {boolean} [keepCase=false] Keeps field casing instead of converting to camel case
         * @property {boolean} [alternateCommentMode=false] Recognize double-slash comments in addition to doc-block comments.
         */ /**
         * Options modifying the behavior of JSON serialization.
         * @interface IToJSONOptions
         * @property {boolean} [keepComments=false] Serializes comments.
         */ /**
         * Parses the given .proto source and returns an object with the parsed contents.
         * @param {string} source Source contents
         * @param {Root} root Root to populate
         * @param {IParseOptions} [options] Parse options. Defaults to {@link parse.defaults} when omitted.
         * @returns {IParserResult} Parser result
         * @property {string} filename=null Currently processing file name for error reporting, if known
         * @property {IParseOptions} defaults Default {@link IParseOptions}
         */function n(e, t, E) {/* istanbul ignore next */function S(e, t, o) { var i = n.filename; return o || (n.filename = null), Error("illegal " + (t || "token") + " '" + e + "' (" + (i ? i + ", " : "") + "line " + H.line + ")") } function I() { var e = [], t; do {/* istanbul ignore if */if ("\"" !== (t = K()) && "'" !== t) throw S(t); e.push(K()), ee(t), t = Z() } while ("\"" === t || "'" === t); return e.join("") } function j(t) { var n = K(); switch (n) { case "'": case "\"": return Q(n), I(); case "true": case "TRUE": return !0; case "false": case "FALSE": return !1; }try { return _(n, !0) } catch (o) {/* istanbul ignore else */if (t && A.test(n)) return n;/* istanbul ignore next */throw S(n, "value") } } function T(e, t) { var n, o; do t && ("\"" === (n = Z()) || "'" === n) ? e.push(I()) : e.push([o = N(K()), ee("to", !0) ? N(K()) : o]); while (ee(",", !0)); ee(";") } function _(e, t) { var n = 1; switch ("-" === e.charAt(0) && (n = -1, e = e.substring(1)), e) { case "inf": case "INF": case "Inf": return n * (1 / 0); case "nan": case "NAN": case "Nan": case "NaN": return NaN; case "0": return 0; }if (m.test(e)) return n * parseInt(e, 10); if (g.test(e)) return n * parseInt(e, 16); if (v.test(e)) return n * parseInt(e, 8);/* istanbul ignore else */if (x.test(e)) return n * parseFloat(e);/* istanbul ignore next */throw S(e, "number", t) } function N(e, t) { switch (e) { case "max": case "MAX": case "Max": return 536870911; case "0": return 0; }/* istanbul ignore if */if (!t && "-" === e.charAt(0)) throw S(e, "id"); if (c.test(e)) return parseInt(e, 10); if (h.test(e)) return parseInt(e, 16);/* istanbul ignore else */if (b.test(e)) return parseInt(e, 8);/* istanbul ignore next */throw S(e, "id") } function R() {/* istanbul ignore if */if (se !== undefined$1) throw S("package");/* istanbul ignore if */if (se = K(), !A.test(se)) throw S(se, "name"); ie = ie.define(se), ee(";") } function w() {
                var e = Z(), t; switch (e) {
                    case "weak": t = de || (de = []), K(); break; case "public": K();// eslint-disable-line no-fallthrough
                    default: t = ae || (ae = []);
                }e = I(), ee(";"), t.push(e)
            } function C() {/* istanbul ignore if */if (ee("="), pe = I(), oe = "proto3" === pe, !oe && "proto2" !== pe) throw S(pe, "syntax"); ee(";") } function D(e, t) { return "option" === t ? (q(e, t), ee(";"), !0) : "message" === t ? (L(e, t), !0) : "enum" === t ? (z(e, t), !0) : "service" === t ? (Y(e, t), !0) : "extend" == t && (G(e, t), !0) } function B(e, t, o) { var i = H.line; if (e && (e.comment = te(), e.filename = n.filename), ee("{", !0)) { for (var r; "}" !== (r = K());)t(r); ee(";", !0) } else o && o(), ee(";"), e && "string" != typeof e.comment && (e.comment = te(i)) } function L(e, t) {/* istanbul ignore if */if (!k.test(t = K())) throw S(t, "type name"); var n = new r(t); B(n, function (e) { if (!D(n, e)) switch (e) { case "map": F(n); break; case "required": case "optional": case "repeated": P(n, e); break; case "oneof": J(n, e); break; case "extensions": T(n.extensions || (n.extensions = [])); break; case "reserved": T(n.reserved || (n.reserved = []), !0); break; default:/* istanbul ignore if */if (!oe || !A.test(e)) throw S(e); Q(e), P(n, "optional"); } }), e.add(n) } function P(e, t, n) { var o = K(); if ("group" === o) return void V(e, t);/* istanbul ignore if */if (!A.test(o)) throw S(o, "type"); var i = K();/* istanbul ignore if */if (!k.test(i)) throw S(i, "name"); i = re(i), ee("="); var r = new s(i, N(K()), o, t, n); B(r, function (e) {/* istanbul ignore else */if ("option" === e) q(r, e), ee(";"); else throw S(e) }, function () { $(r) }), e.add(r), !oe && r.repeated && (y.packed[o] !== undefined$1 || y.basic[o] === undefined$1) && r.setOption("packed", !1, !0) } function V(e, t) {
                var o = K();/* istanbul ignore if */if (!k.test(o)) throw S(o, "name"); var i = f.lcFirst(o); o === i && (o = f.ucFirst(o)), ee("="); var a = N(K()), d = new r(o); d.group = !0; var p = new s(i, a, o, t); p.filename = n.filename, B(d, function (e) {
                    switch (e) {
                        case "option": q(d, e), ee(";"); break; case "required": case "optional": case "repeated": P(d, e); break;/* istanbul ignore next */default: throw S(e);// there are no groups with proto3 semantics
                    }
                }), e.add(d).add(p)
            } function F(e) { ee("<"); var t = K();/* istanbul ignore if */if (y.mapKey[t] === undefined$1) throw S(t, "type"); ee(","); var n = K();/* istanbul ignore if */if (!A.test(n)) throw S(n, "type"); ee(">"); var o = K();/* istanbul ignore if */if (!k.test(o)) throw S(o, "name"); ee("="); var i = new a(re(o), N(K()), t, n); B(i, function (e) {/* istanbul ignore else */if ("option" === e) q(i, e), ee(";"); else throw S(e) }, function () { $(i) }), e.add(i) } function J(e, t) {/* istanbul ignore if */if (!k.test(t = K())) throw S(t, "name"); var n = new d(re(t)); B(n, function (e) { "option" === e ? (q(n, e), ee(";")) : (Q(e), P(n, "optional")) }), e.add(n) } function z(e, t) {/* istanbul ignore if */if (!k.test(t = K())) throw S(t, "name"); var n = new p(t); B(n, function (e) { "option" === e ? (q(n, e), ee(";")) : "reserved" === e ? T(n.reserved || (n.reserved = []), !0) : M(n, e) }), e.add(n) } function M(e, t) {/* istanbul ignore if */if (!k.test(t)) throw S(t, "name"); ee("="); var n = N(K(), !0), o = {}; B(o, function (e) {/* istanbul ignore else */if ("option" === e) q(o, e), ee(";"); else throw S(e) }, function () { $(o) }), e.add(t, n, o.comment) } function q(e, t) { var n = ee("(", !0);/* istanbul ignore if */if (!A.test(t = K())) throw S(t, "name"); var o = t; n && (ee(")"), o = "(" + o + ")", t = Z(), O.test(t) && (o += t, K())), ee("="), W(e, o) } function W(e, t) {
                if (ee("{", !0))// { a: "foo" b { c: "bar" } }
                    do {/* istanbul ignore if */if (!k.test(le = K())) throw S(le, "name"); "{" === Z() ? W(e, t + "." + le) : (ee(":"), "{" === Z() ? W(e, t + "." + le) : X(e, t + "." + le, j(!0))), ee(",", !0) } while (!ee("}", !0)); else X(e, t, j(!0));// Does not enforce a delimiter to be universal
            } function X(e, t, n) { e.setOption && e.setOption(t, n) } function $(e) { if (ee("[", !0)) { do q(e, "option"); while (ee(",", !0)); ee("]") } return e } function Y(e, t) {/* istanbul ignore if */if (!k.test(t = K())) throw S(t, "service name"); var n = new l(t); B(n, function (e) { if (!D(n, e))/* istanbul ignore else */if ("rpc" === e) U(n, e); else throw S(e) }), e.add(n) } function U(e, t) { var n = t;/* istanbul ignore if */if (!k.test(t = K())) throw S(t, "name"); var o = t, i, r, s, a;/* istanbul ignore if */if (ee("("), ee("stream", !0) && (r = !0), !A.test(t = K())) throw S(t);/* istanbul ignore if */if (i = t, ee(")"), ee("returns"), ee("("), ee("stream", !0) && (a = !0), !A.test(t = K())) throw S(t); s = t, ee(")"); var d = new u(o, n, i, s, r, a); B(d, function (e) {/* istanbul ignore else */if ("option" === e) q(d, e), ee(";"); else throw S(e) }), e.add(d) } function G(e, t) {/* istanbul ignore if */if (!A.test(t = K())) throw S(t, "reference"); var n = t; B(null, function (t) { switch (t) { case "required": case "repeated": case "optional": P(e, t, n); break; default:/* istanbul ignore if */if (!oe || !A.test(t)) throw S(t); Q(t), P(e, "optional", n); } }) } t instanceof i || (E = t, t = new i), E || (E = n.defaults); for (var H = o(e, E.alternateCommentMode || !1), K = H.next, Q = H.push, Z = H.peek, ee = H.skip, te = H.cmnt, ne = !0, oe = !1, ie = t, re = E.keepCase ? function (e) { return e } : f.camelCase, se, ae, de, pe, le; null !== (le = K());)switch (le) { case "package":/* istanbul ignore if */if (!ne) throw S(le); R(); break; case "import":/* istanbul ignore if */if (!ne) throw S(le); w(); break; case "syntax":/* istanbul ignore if */if (!ne) throw S(le); C(); break; case "option":/* istanbul ignore if */if (!ne) throw S(le); q(ie, le), ee(";"); break; default:/* istanbul ignore else */if (D(ie, le)) { ne = !1; continue }/* istanbul ignore next */throw S(le); }return n.filename = null, { package: se, imports: ae, weakImports: de, syntax: pe, root: t }
            }/**
         * Parses the given .proto source and returns an object with the parsed contents.
         * @name parse
         * @function
         * @param {string} source Source contents
         * @param {IParseOptions} [options] Parse options. Defaults to {@link parse.defaults} when omitted.
         * @returns {IParserResult} Parser result
         * @property {string} filename=null Currently processing file name for error reporting, if known
         * @property {IParseOptions} defaults Default {@link IParseOptions}
         * @variation 2
         */t.exports = n, n.filename = null, n.defaults = { keepCase: !1 }; var o = e(34), i = e(29), r = e(35), s = e(16), a = e(20), d = e(25), p = e(15), l = e(33), u = e(22), y = e(36), f = e(37), m = /^[1-9][0-9]*$/, c = /^-?[1-9][0-9]*$/, g = /^0[x][0-9a-fA-F]+$/, h = /^-?0[x][0-9a-fA-F]+$/, v = /^0[0-7]+$/, b = /^-?0[0-7]+$/, x = /^(?![eE])[0-9]*(?:\.[0-9]*)?(?:[eE][+-]?[0-9]+)?$/, k = /^[a-zA-Z_][a-zA-Z_0-9]*$/, A = /^(?:\.?[a-zA-Z_][a-zA-Z_0-9]*)(?:\.[a-zA-Z_][a-zA-Z_0-9]*)*$/, O = /^(?:\.[a-zA-Z_][a-zA-Z_0-9]*)+$/
        }, { 15: 15, 16: 16, 20: 20, 22: 22, 25: 25, 29: 29, 33: 33, 34: 34, 35: 35, 36: 36, 37: 37 }], 27: [function (e, t) {/* istanbul ignore next */function n(e, t) { return RangeError("index out of range: " + e.pos + " + " + (t || 1) + " > " + e.len) }/**
         * Constructs a new reader instance using the specified buffer.
         * @classdesc Wire format reader using `Uint8Array` if available, otherwise `Array`.
         * @constructor
         * @param {Uint8Array} buffer Buffer to read from
         */function o(e) {/**
             * Read buffer.
             * @type {Uint8Array}
             */ /**
             * Read buffer position.
             * @type {number}
             */ /**
             * Read buffer length.
             * @type {number}
             */this.buf = e, this.pos = 0, this.len = e.length
            }/* eslint-disable no-invalid-this */function i() {// tends to deopt with local vars for octet etc.
                var e = new d(0, 0), t = 0; if (4 < this.len - this.pos) {// fast route (lo)
                    for (; 4 > t; ++t)if (e.lo = (e.lo | (127 & this.buf[this.pos]) << 7 * t) >>> 0, 128 > this.buf[this.pos++]) return e;// 5th
                    if (e.lo = (e.lo | (127 & this.buf[this.pos]) << 28) >>> 0, e.hi = (e.hi | (127 & this.buf[this.pos]) >> 4) >>> 0, 128 > this.buf[this.pos++]) return e; t = 0
                } else {
                    for (; 3 > t; ++t) {/* istanbul ignore if */if (this.pos >= this.len) throw n(this);// 1st..3th
                        if (e.lo = (e.lo | (127 & this.buf[this.pos]) << 7 * t) >>> 0, 128 > this.buf[this.pos++]) return e
                    }// 4th
                    return e.lo = (e.lo | (127 & this.buf[this.pos++]) << 7 * t) >>> 0, e
                } if (4 < this.len - this.pos) {// fast route (hi)
                    for (; 5 > t; ++t)if (e.hi = (e.hi | (127 & this.buf[this.pos]) << 7 * t + 3) >>> 0, 128 > this.buf[this.pos++]) return e;
                } else for (; 5 > t; ++t) {/* istanbul ignore if */if (this.pos >= this.len) throw n(this);// 6th..10th
                    if (e.hi = (e.hi | (127 & this.buf[this.pos]) << 7 * t + 3) >>> 0, 128 > this.buf[this.pos++]) return e
                }/* istanbul ignore next */throw Error("invalid varint encoding")
            }/* eslint-enable no-invalid-this */ /**
         * Reads a varint as a signed 64 bit value.
         * @name Reader#int64
         * @function
         * @returns {Long} Value read
         */ /**
         * Reads a varint as an unsigned 64 bit value.
         * @name Reader#uint64
         * @function
         * @returns {Long} Value read
         */ /**
         * Reads a zig-zag encoded varint as a signed 64 bit value.
         * @name Reader#sint64
         * @function
         * @returns {Long} Value read
         */ /**
         * Reads a varint as a boolean.
         * @returns {boolean} Value read
         */function r(e, t) {// note that this uses `end`, not `pos`
                return (e[t - 4] | e[t - 3] << 8 | e[t - 2] << 16 | e[t - 1] << 24) >>> 0
            }/**
         * Reads fixed 32 bits as an unsigned 32 bit integer.
         * @returns {number} Value read
         */ /* eslint-disable no-invalid-this */function s()/* this: Reader */ {/* istanbul ignore if */if (this.pos + 8 > this.len) throw n(this, 8); return new d(r(this.buf, this.pos += 4), r(this.buf, this.pos += 4)) }/* eslint-enable no-invalid-this */ /**
         * Reads fixed 64 bits.
         * @name Reader#fixed64
         * @function
         * @returns {Long} Value read
         */ /**
         * Reads zig-zag encoded fixed 64 bits.
         * @name Reader#sfixed64
         * @function
         * @returns {Long} Value read
         */ /**
         * Reads a float (32 bit) as a number.
         * @function
         * @returns {number} Value read
         */t.exports = o; var a = e(39), d = a.LongBits, p = a.utf8, l = "undefined" == typeof Uint8Array ?/* istanbul ignore next */function (e) { if (Array.isArray(e)) return new o(e); throw Error("illegal buffer") } : function (e) { if (e instanceof Uint8Array || Array.isArray(e)) return new o(e); throw Error("illegal buffer") }, u;/**
         * Creates a new reader using the specified buffer.
         * @function
         * @param {Uint8Array|Buffer} buffer Buffer to read from
         * @returns {Reader|BufferReader} A {@link BufferReader} if `buffer` is a Buffer, otherwise a {@link Reader}
         * @throws {Error} If `buffer` is not a valid buffer
         */ /**
         * Reads a varint as an unsigned 32 bit value.
         * @function
         * @returns {number} Value read
         */ /**
         * Reads a varint as a signed 32 bit value.
         * @returns {number} Value read
         */ /**
         * Reads a zig-zag encoded varint as a signed 32 bit value.
         * @returns {number} Value read
         */ /**
         * Reads fixed 32 bits as a signed 32 bit integer.
         * @returns {number} Value read
         */ /**
         * Reads a double (64 bit float) as a number.
         * @function
         * @returns {number} Value read
         */ /**
         * Reads a sequence of bytes preceeded by its length as a varint.
         * @returns {Uint8Array} Value read
         */ /**
         * Reads a string preceeded by its byte length as a varint.
         * @returns {string} Value read
         */ /**
         * Skips the specified number of bytes if specified, otherwise skips a varint.
         * @param {number} [length] Length if known, otherwise a varint is assumed
         * @returns {Reader} `this`
         */ /**
         * Skips the next element of the specified wire type.
         * @param {number} wireType Wire type received
         * @returns {Reader} `this`
         */o.create = a.Buffer ? function (e) { return (o.create = function (e) { return a.Buffer.isBuffer(e) ? new u(e)/* istanbul ignore next */ : l(e) })(e) }/* istanbul ignore next */ : l, o.prototype._slice = a.Array.prototype.subarray ||/* istanbul ignore next */a.Array.prototype.slice, o.prototype.uint32 = function () {
                var e = 4294967295;// optimizer type-hint, tends to deopt otherwise (?!)
                return function () { if (e = (127 & this.buf[this.pos]) >>> 0, 128 > this.buf[this.pos++]) return e; if (e = (e | (127 & this.buf[this.pos]) << 7) >>> 0, 128 > this.buf[this.pos++]) return e; if (e = (e | (127 & this.buf[this.pos]) << 14) >>> 0, 128 > this.buf[this.pos++]) return e; if (e = (e | (127 & this.buf[this.pos]) << 21) >>> 0, 128 > this.buf[this.pos++]) return e; if (e = (e | (15 & this.buf[this.pos]) << 28) >>> 0, 128 > this.buf[this.pos++]) return e;/* istanbul ignore if */if ((this.pos += 5) > this.len) throw this.pos = this.len, n(this, 10); return e }
            }(), o.prototype.int32 = function () { return 0 | this.uint32() }, o.prototype.sint32 = function () { var e = this.uint32(); return 0 | e >>> 1 ^ -(1 & e) }, o.prototype.bool = function () { return 0 !== this.uint32() }, o.prototype.fixed32 = function () {/* istanbul ignore if */if (this.pos + 4 > this.len) throw n(this, 4); return r(this.buf, this.pos += 4) }, o.prototype.sfixed32 = function () {/* istanbul ignore if */if (this.pos + 4 > this.len) throw n(this, 4); return 0 | r(this.buf, this.pos += 4) }, o.prototype.float = function () {/* istanbul ignore if */if (this.pos + 4 > this.len) throw n(this, 4); var e = a.float.readFloatLE(this.buf, this.pos); return this.pos += 4, e }, o.prototype.double = function () {/* istanbul ignore if */if (this.pos + 8 > this.len) throw n(this, 4); var e = a.float.readDoubleLE(this.buf, this.pos); return this.pos += 8, e }, o.prototype.bytes = function () {
                var e = this.uint32(), t = this.pos, o = this.pos + e;/* istanbul ignore if */if (o > this.len) throw n(this, e); return this.pos += e, Array.isArray(this.buf) ? this.buf.slice(t, o) : t === o// fix for IE 10/Win8 and others' subarray returning array of size 1
                    ? new this.buf.constructor(0) : this._slice.call(this.buf, t, o)
            }, o.prototype.string = function () { var e = this.bytes(); return p.read(e, 0, e.length) }, o.prototype.skip = function (e) { if ("number" == typeof e) {/* istanbul ignore if */if (this.pos + e > this.len) throw n(this, e); this.pos += e } else do/* istanbul ignore if */if (this.pos >= this.len) throw n(this); while (128 & this.buf[this.pos++]); return this }, o.prototype.skipType = function (e) { switch (e) { case 0: this.skip(); break; case 1: this.skip(8); break; case 2: this.skip(this.uint32()); break; case 3: for (; 4 != (e = 7 & this.uint32());)this.skipType(e); break; case 5: this.skip(4); break;/* istanbul ignore next */default: throw Error("invalid wire type " + e + " at offset " + this.pos); }return this }, o._configure = function (e) { u = e; var t = a.Long ? "toLong" :/* istanbul ignore next */"toNumber"; a.merge(o.prototype, { int64: function () { return i.call(this)[t](!1) }, uint64: function () { return i.call(this)[t](!0) }, sint64: function () { return i.call(this).zzDecode()[t](!1) }, fixed64: function () { return s.call(this)[t](!0) }, sfixed64: function () { return s.call(this)[t](!1) } }) }
        }, { 39: 39 }], 28: [function (e, t) {/**
         * Constructs a new buffer reader instance.
         * @classdesc Wire format reader using node buffers.
         * @extends Reader
         * @constructor
         * @param {Buffer} buffer Buffer to read from
         */function n(e) { o.call(this, e) }/* istanbul ignore else */t.exports = n;// extends Reader
            var o = e(27); (n.prototype = Object.create(o.prototype)).constructor = n; var i = e(39);/**
         * @override
         */i.Buffer && (n.prototype._slice = i.Buffer.prototype.slice), n.prototype.string = function () {
                var e = this.uint32();// modifies pos
                return this.buf.utf8Slice(this.pos, this.pos = _Mathmin(this.pos + e, this.len))
            }
        }, { 27: 27, 39: 39 }], 29: [function (e, t) {// "
/**
         * Constructs a new root namespace instance.
         * @classdesc Root namespace wrapping all types, enums, services, sub-namespaces etc. that belong together.
         * @extends NamespaceBase
         * @constructor
         * @param {Object.<string,*>} [options] Top level options
         */function n(e) {/**
             * Deferred extension fields.
             * @type {Field[]}
             */ /**
             * Resolved file names of loaded files.
             * @type {string[]}
             */s.call(this, "", e), this.deferred = [], this.files = []
            }/**
         * Loads a namespace descriptor into a root namespace.
         * @param {INamespace} json Nameespace descriptor
         * @param {Root} [root] Root namespace, defaults to create a new one if omitted
         * @returns {Root} Root namespace
         */ // A symbol-like function to safely signal synchronous loading
/* istanbul ignore next */function o() { }// eslint-disable-line no-empty-function
/**
         * Loads one or multiple .proto or preprocessed .json files into this root namespace and calls the callback.
         * @param {string|string[]} filename Names of one or multiple files to load
         * @param {IParseOptions} options Parse options
         * @param {LoadCallback} callback Callback function
         * @returns {undefined}
         */ /**
         * Handles a deferred declaring extension field by creating a sister field to represent it within its extended type.
         * @param {Root} root Root instance
         * @param {Field} field Declaring extension field witin the declaring type
         * @returns {boolean} `true` if successfully added to the extended type, `false` otherwise
         * @inner
         * @ignore
         */function r(e, t) { var n = t.parent.lookup(t.extend); if (n) { var o = new a(t.fullName, t.id, t.type, t.rule, undefined$1, t.options); return o.declaringField = t, t.extensionField = o, n.add(o), !0 } return !1 }/**
         * Called when any object is added to this root or its sub-namespaces.
         * @param {ReflectionObject} object Object added
         * @returns {undefined}
         * @private
         */t.exports = n;// extends Namespace
            var s = e(23); ((n.prototype = Object.create(s.prototype)).constructor = n).className = "Root"; var a = e(16), d = e(15), p = e(25), l = e(37), u,// cyclic
                y,// might be excluded
                f; n.fromJSON = function (e, t) { return t || (t = new n), e.options && t.setOptions(e.options), t.addJSON(e.nested) }, n.prototype.resolvePath = l.path.resolve, n.prototype.load = function e(t, n, r) {// undocumented
                    // Finishes loading by calling the callback (exactly once)
                    function s(e, t) {/* istanbul ignore if */if (r) { var n = r; if (r = null, u) throw e; n(e, t) } }// Processes a single file
                    function a(e, t) { try { if (l.isString(t) && "{" === t.charAt(0) && (t = JSON.parse(t)), !l.isString(t)) p.setOptions(t.options).addJSON(t.nested); else { y.filename = e; var o = y(t, p, n), r = 0, a; if (o.imports) for (; r < o.imports.length; ++r)(a = p.resolvePath(e, o.imports[r])) && d(a); if (o.weakImports) for (r = 0; r < o.weakImports.length; ++r)(a = p.resolvePath(e, o.weakImports[r])) && d(a, !0) } } catch (e) { s(e) } u || m || s(null, p) }// Fetches a single file
                    function d(e, t) {// Strip path if this file references a bundled definition
                        var n = e.lastIndexOf("google/protobuf/"); if (-1 < n) { var o = e.substring(n); o in f && (e = o) }// Skip if already loaded / attempted
                        if (!(-1 < p.files.indexOf(e))) {// Shortcut bundled definitions
                            if (p.files.push(e), e in f) return void (u ? a(e, f[e]) : (++m, setTimeout(function () { --m, a(e, f[e]) })));// Otherwise fetch from disk or network
                            if (u) { var i; try { i = l.fs.readFileSync(e).toString("utf8") } catch (e) { return void (t || s(e)) } a(e, i) } else ++m, l.fetch(e, function (n, o) {/* istanbul ignore if */return --m, r ? n ? void (t ? !m &&// can't be covered reliably
                                s(null, p) : s(n)) : void a(e, o) : void 0;// terminated meanwhile
                            })
                        }
                    } "function" == typeof n && (r = n, n = undefined$1); var p = this; if (!r) return l.asPromise(e, p, t, n); var u = r === o, m = 0; l.isString(t) && (t = [t]); for (var c = 0, g; c < t.length; ++c)(g = p.resolvePath("", t[c])) && d(g); return u ? p : (m || s(null, p), undefined$1)
                }, n.prototype.loadSync = function (e, t) { if (!l.isNode) throw Error("not supported"); return this.load(e, t, o) }, n.prototype.resolveAll = function () { if (this.deferred.length) throw Error("unresolvable extensions: " + this.deferred.map(function (e) { return "'extend " + e.extend + "' in " + e.parent.fullName }).join(", ")); return s.prototype.resolveAll.call(this) };// only uppercased (and thus conflict-free) children are exposed, see below
            var m = /^[A-Z]/;/**
         * Called when any object is removed from this root or its sub-namespaces.
         * @param {ReflectionObject} object Object removed
         * @returns {undefined}
         * @private
         */ // Sets up cyclic dependencies (called in index-light)
            n.prototype._handleAdd = function (e) {
                if (e instanceof a)/* an extension field (implies not part of a oneof) */e.extend === undefined$1 || e.extensionField || r(this, e) || this.deferred.push(e); else if (e instanceof d) m.test(e.name) && (e.parent[e.name] = e.values); else if (!(e instanceof p))/* everything else is a namespace */ {
                    if (e instanceof u)// Try to handle any deferred extensions
                        for (var t = 0; t < this.deferred.length;)r(this, this.deferred[t]) ? this.deferred.splice(t, 1) : ++t; for (var n = 0; n </* initializes */e.nestedArray.length; ++n)// recurse into the namespace
                        this._handleAdd(e._nestedArray[n]); m.test(e.name) && (e.parent[e.name] = e)
                }// The above also adds uppercased (and thus conflict-free) nested types, services and enums as
                // properties of namespaces just like static code does. This allows using a .d.ts generated for
                // a static module with reflection-based solutions where the condition is met.
            }, n.prototype._handleRemove = function (e) {
                if (e instanceof a) {
                    if (/* an extension field */e.extend !== undefined$1) if (/* already handled */e.extensionField)// remove its sister field
                        e.extensionField.parent.remove(e.extensionField), e.extensionField = null; else {// cancel the extension
                        var t = this.deferred.indexOf(e);/* istanbul ignore else */-1 < t && this.deferred.splice(t, 1)
                    }
                } else if (e instanceof d) m.test(e.name) && delete e.parent[e.name]; else if (e instanceof s) {
                    for (var n = 0; n </* initializes */e.nestedArray.length; ++n)// recurse into the namespace
                        this._handleRemove(e._nestedArray[n]); m.test(e.name) && delete e.parent[e.name]
                }
            }, n._configure = function (e, t, n) { u = e, y = t, f = n }
        }, { 15: 15, 16: 16, 23: 23, 25: 25, 37: 37 }], 30: [function (e, t) { t.exports = {} }, {}], 31: [function (e, t, n) {/**
         * Streaming RPC helpers.
         * @namespace
         */ /**
         * RPC implementation passed to {@link Service#create} performing a service request on network level, i.e. by utilizing http requests or websockets.
         * @typedef RPCImpl
         * @type {function}
         * @param {Method|rpc.ServiceMethod<Message<{}>,Message<{}>>} method Reflected or static method being called
         * @param {Uint8Array} requestData Request data
         * @param {RPCImplCallback} callback Callback function
         * @returns {undefined}
         * @example
         * function rpcImpl(method, requestData, callback) {
         *     if (protobuf.util.lcFirst(method.name) !== "myMethod") // compatible with static code
         *         throw Error("no such method");
         *     asynchronouslyObtainAResponse(requestData, function(err, responseData) {
         *         callback(err, responseData);
         *     });
         * }
         */ /**
         * Node-style callback as used by {@link RPCImpl}.
         * @typedef RPCImplCallback
         * @type {function}
         * @param {Error|null} error Error, if any, otherwise `null`
         * @param {Uint8Array|null} [response] Response data or `null` to signal end of stream, if there hasn't been an error
         * @returns {undefined}
         */n.Service = e(32)
        }, { 32: 32 }], 32: [function (e, t) {/**
         * A service method callback as used by {@link rpc.ServiceMethod|ServiceMethod}.
         *
         * Differs from {@link RPCImplCallback} in that it is an actual callback of a service method which may not return `response = null`.
         * @typedef rpc.ServiceMethodCallback
         * @template TRes extends Message<TRes>
         * @type {function}
         * @param {Error|null} error Error, if any
         * @param {TRes} [response] Response message
         * @returns {undefined}
         */ /**
         * A service method part of a {@link rpc.Service} as created by {@link Service.create}.
         * @typedef rpc.ServiceMethod
         * @template TReq extends Message<TReq>
         * @template TRes extends Message<TRes>
         * @type {function}
         * @param {TReq|Properties<TReq>} request Request message or plain object
         * @param {rpc.ServiceMethodCallback<TRes>} [callback] Node-style callback called with the error, if any, and the response message
         * @returns {Promise<Message<TRes>>} Promise if `callback` has been omitted, otherwise `undefined`
         */ /**
         * Constructs a new RPC service instance.
         * @classdesc An RPC service as returned by {@link Service#create}.
         * @exports rpc.Service
         * @extends util.EventEmitter
         * @constructor
         * @param {RPCImpl} rpcImpl RPC implementation
         * @param {boolean} [requestDelimited=false] Whether requests are length-delimited
         * @param {boolean} [responseDelimited=false] Whether responses are length-delimited
         */function n(e, t, n) {
                if ("function" != typeof e) throw TypeError("rpcImpl must be a function");/**
             * RPC implementation. Becomes `null` once the service is ended.
             * @type {RPCImpl|null}
             */ /**
             * Whether requests are length-delimited.
             * @type {boolean}
             */ /**
             * Whether responses are length-delimited.
             * @type {boolean}
             */o.EventEmitter.call(this), this.rpcImpl = e, this.requestDelimited = !!t, this.responseDelimited = !!n
            }/**
         * Calls a service method through {@link rpc.Service#rpcImpl|rpcImpl}.
         * @param {Method|rpc.ServiceMethod<TReq,TRes>} method Reflected or static method
         * @param {Constructor<TReq>} requestCtor Request constructor
         * @param {Constructor<TRes>} responseCtor Response constructor
         * @param {TReq|Properties<TReq>} request Request message or plain object
         * @param {rpc.ServiceMethodCallback<TRes>} callback Service callback
         * @returns {undefined}
         * @template TReq extends Message<TReq>
         * @template TRes extends Message<TRes>
         */t.exports = n; var o = e(39);// Extends EventEmitter
/**
         * Ends this service and emits the `end` event.
         * @param {boolean} [endedByRPC=false] Whether the service has been ended by the RPC implementation.
         * @returns {rpc.Service} `this`
         */(n.prototype = Object.create(o.EventEmitter.prototype)).constructor = n, n.prototype.rpcCall = function e(t, n, i, r, s) { if (!r) throw TypeError("request must be specified"); var a = this; if (!s) return o.asPromise(e, a, t, n, i, r); if (!a.rpcImpl) return setTimeout(function () { s(Error("already ended")) }, 0), undefined$1; try { return a.rpcImpl(t, n[a.requestDelimited ? "encodeDelimited" : "encode"](r).finish(), function (e, n) { if (e) return a.emit("error", e, t), s(e); if (null === n) return a.end(!0), undefined$1; if (!(n instanceof i)) try { n = i[a.responseDelimited ? "decodeDelimited" : "decode"](n) } catch (e) { return a.emit("error", e, t), s(e) } return a.emit("data", n, t), s(null, n) }) } catch (e) { return a.emit("error", e, t), setTimeout(function () { s(e) }, 0), undefined$1 } }, n.prototype.end = function (e) {
                return this.rpcImpl && (!e &&// signal end to rpcImpl
                    this.rpcImpl(null, null, null), this.rpcImpl = null, this.emit("end").off()), this
            }
        }, { 39: 39 }], 33: [function (e, t) {/**
         * Constructs a new service instance.
         * @classdesc Reflected service.
         * @extends NamespaceBase
         * @constructor
         * @param {string} name Service name
         * @param {Object.<string,*>} [options] Service options
         * @throws {TypeError} If arguments are invalid
         */function n(e, t) {/**
             * Service methods.
             * @type {Object.<string,Method>}
             */ // toJSON, marker
/**
             * Cached methods as an array.
             * @type {Method[]|null}
             * @private
             */r.call(this, e, t), this.methods = {}, this._methodsArray = null
            }/**
         * Service descriptor.
         * @interface IService
         * @extends INamespace
         * @property {Object.<string,IMethod>} methods Method descriptors
         */ /**
         * Constructs a service from a service descriptor.
         * @param {string} name Service name
         * @param {IService} json Service descriptor
         * @returns {Service} Created service
         * @throws {TypeError} If arguments are invalid
         */function o(e) { return e._methodsArray = null, e }/**
         * @override
         */t.exports = n;// extends Namespace
            var r = e(23); ((n.prototype = Object.create(r.prototype)).constructor = n).className = "Service"; var s = e(22), a = e(37), d = e(31);/**
         * Converts this service to a service descriptor.
         * @param {IToJSONOptions} [toJSONOptions] JSON conversion options
         * @returns {IService} Service descriptor
         */ /**
         * Methods of this service as an array for iteration.
         * @name Service#methodsArray
         * @type {Method[]}
         * @readonly
         */ /**
         * @override
         */ /**
         * @override
         */ /**
         * @override
         */ /**
         * Creates a runtime service using the specified rpc implementation.
         * @param {RPCImpl} rpcImpl RPC implementation
         * @param {boolean} [requestDelimited=false] Whether requests are length-delimited
         * @param {boolean} [responseDelimited=false] Whether responses are length-delimited
         * @returns {rpc.Service} RPC service. Useful where requests and/or responses are streamed.
         */n.fromJSON = function (e, t) { var o = new n(e, t.options);/* istanbul ignore else */if (t.methods) for (var r = Object.keys(t.methods), a = 0; a < r.length; ++a)o.add(s.fromJSON(r[a], t.methods[r[a]])); return t.nested && o.addJSON(t.nested), o.comment = t.comment, o }, n.prototype.toJSON = function (e) { var t = r.prototype.toJSON.call(this, e), n = !!e && !!e.keepComments; return a.toObject(["options", t && t.options || undefined$1, "methods", r.arrayToJSON(this.methodsArray, e) ||/* istanbul ignore next */{}, "nested", t && t.nested || undefined$1, "comment", n ? this.comment : undefined$1]) }, Object.defineProperty(n.prototype, "methodsArray", { get: function () { return this._methodsArray || (this._methodsArray = a.toArray(this.methods)) } }), n.prototype.get = function (e) { return this.methods[e] || r.prototype.get.call(this, e) }, n.prototype.resolveAll = function () { for (var e = this.methodsArray, t = 0; t < e.length; ++t)e[t].resolve(); return r.prototype.resolve.call(this) }, n.prototype.add = function (e) {/* istanbul ignore if */if (this.get(e.name)) throw Error("duplicate name '" + e.name + "' in " + this); return e instanceof s ? (this.methods[e.name] = e, e.parent = this, o(this)) : r.prototype.add.call(this, e) }, n.prototype.remove = function (e) { if (e instanceof s) {/* istanbul ignore if */if (this.methods[e.name] !== e) throw Error(e + " is not a member of " + this); return delete this.methods[e.name], e.parent = null, o(this) } return r.prototype.remove.call(this, e) }, n.prototype.create = function (e, t, n) { for (var o = new d.Service(e, t, n), r = 0, s, p; r </* initializes */this.methodsArray.length; ++r)p = a.lcFirst((s = this._methodsArray[r]).resolve().name).replace(/[^$\w_]/g, ""), o[p] = a.codegen(["r", "c"], a.isReserved(p) ? p + "_" : p)("return this.rpcCall(m,q,s,r,c)")({ m: s, q: s.resolvedRequestType.ctor, s: s.resolvedResponseType.ctor }); return o }
        }, { 22: 22, 23: 23, 31: 31, 37: 37 }], 34: [function (e, t) {/**
         * Unescapes a string.
         * @param {string} str String to unescape
         * @returns {string} Unescaped string
         * @property {Object.<string,string>} map Special characters map
         * @memberof tokenize
         */function n(e) { return e.replace(u, function (e, t) { return "\\" === t || "" === t ? t : y[t] || "" }) }/**
         * Gets the next token and advances.
         * @typedef TokenizerHandleNext
         * @type {function}
         * @returns {string|null} Next token or `null` on eof
         */ /**
         * Peeks for the next token.
         * @typedef TokenizerHandlePeek
         * @type {function}
         * @returns {string|null} Next token or `null` on eof
         */ /**
         * Pushes a token back to the stack.
         * @typedef TokenizerHandlePush
         * @type {function}
         * @param {string} token Token
         * @returns {undefined}
         */ /**
         * Skips the next token.
         * @typedef TokenizerHandleSkip
         * @type {function}
         * @param {string} expected Expected token
         * @param {boolean} [optional=false] If optional
         * @returns {boolean} Whether the token matched
         * @throws {Error} If the token didn't match and is not optional
         */ /**
         * Gets the comment on the previous line or, alternatively, the line comment on the specified line.
         * @typedef TokenizerHandleCmnt
         * @type {function}
         * @param {number} [line] Line number
         * @returns {string|null} Comment text or `null` if none
         */ /**
         * Handle object returned from {@link tokenize}.
         * @interface ITokenizerHandle
         * @property {TokenizerHandleNext} next Gets the next token and advances (`null` on eof)
         * @property {TokenizerHandlePeek} peek Peeks for the next token (`null` on eof)
         * @property {TokenizerHandlePush} push Pushes a token back to the stack
         * @property {TokenizerHandleSkip} skip Skips a token, returns its presence and advances or, if non-optional and not present, throws
         * @property {TokenizerHandleCmnt} cmnt Gets the comment on the previous line or the line comment on the specified line, if any
         * @property {number} line Current line number
         */ /**
         * Tokenizes the given .proto source and returns an object with useful utility functions.
         * @param {string} source Source contents
         * @param {boolean} alternateCommentMode Whether we should activate alternate comment parsing mode.
         * @returns {ITokenizerHandle} Tokenizer handle
         */function o(e, t) {/* istanbul ignore next */ /**
             * Creates an error for illegal syntax.
             * @param {string} subject Subject
             * @returns {Error} Error created
             * @inner
             */function o(e) { return Error("illegal " + e + " (line " + A + ")") }/**
             * Reads a string till its end.
             * @returns {string} String read
             * @inner
             */function u() { var t = "'" === T ? s : r; t.lastIndex = x - 1; var i = t.exec(e); if (!i) throw o("string"); return x = t.lastIndex, h(T), T = null, n(i[1]) }/**
             * Gets the character at `pos` within the source.
             * @param {number} pos Position
             * @returns {string} Character
             * @inner
             */function y(t) { return e.charAt(t) }/**
             * Sets the current comment text.
             * @param {number} start Start offset
             * @param {number} end End offset
             * @returns {undefined}
             * @inner
             */function f(n, o) { O = e.charAt(n++), S = A, I = !1; var r = t ? 2 : 3; var s = n - r, l; do if (0 > --s || "\n" === (l = e.charAt(s))) { I = !0; break } while (" " === l || "\t" === l); for (var u = e.substring(n, o).split(p), y = 0; y < u.length; ++y)u[y] = u[y].replace(t ? d : a, "").trim(); E = u.join("\n").trim() } function m(t) {
                    var n = c(t), o = e.substring(t, n), i = /^\s*\/{1,2}/.test(o);// see if remaining line matches comment pattern
                    return i
                } function c(e) {// find end of cursor's line
                    for (var t = e; t < k && "\n" !== y(t);)t++; return t
                }/**
             * Obtains the next token.
             * @returns {string|null} Next token or `null` on eof
             * @inner
             */function g() {
                    if (0 < j.length) return j.shift(); if (T) return u(); var n, r, s, a, d; do {
                        if (x === k) return null; for (n = !1; l.test(s = y(x));)if ("\n" === s && ++A, ++x === k) return null; if ("/" === y(x)) {
                            if (++x === k) throw o("comment"); if ("/" === y(x)) {// Line
                                if (!t) { for (d = "/" === y(a = x + 1); "\n" !== y(++x);)if (x === k) return null; ++x, d && f(a, x - 1), ++A, n = !0 } else { if (a = x, d = !1, m(x)) { d = !0; do { if (x = c(x), x === k) break; x++ } while (m(x)) } else x = _Mathmin(k, c(x) + 1); d && f(a, x), A++ , n = !0 }
                            } else if ("*" === (s = y(x))) { a = x + 1, d = t || "*" === y(a); do { if ("\n" === s && ++A, ++x === k) throw o("comment"); r = s, s = y(x) } while ("*" !== r || "/" !== s); ++x, d && f(a, x - 2), n = !0 } else return "/"
                        }
                    } while (n);// offset !== length if we got here
                    var p = x; i.lastIndex = 0; var g = i.test(y(p++)); if (!g) for (; p < k && !i.test(y(p));)++p; var h = e.substring(x, x = p); return ("\"" === h || "'" === h) && (T = h), h
                }/**
             * Pushes a token back to the stack.
             * @param {string} token Token
             * @returns {undefined}
             * @inner
             */function h(e) { j.push(e) }/**
             * Peeks for the next token.
             * @returns {string|null} Token or `null` on eof
             * @inner
             */function v() { if (!j.length) { var e = g(); if (null === e) return null; h(e) } return j[0] }/**
             * Skips a token.
             * @param {string} expected Expected token
             * @param {boolean} [optional=false] Whether the token is optional
             * @returns {boolean} `true` when skipped, `false` if not
             * @throws {Error} When a required token is not present
             * @inner
             */ /**
             * Gets a comment.
             * @param {number} [trailingLine] Line number if looking for a trailing comment
             * @returns {string|null} Comment text
             * @inner
             */function b(e) { var n = null; return e === undefined$1 ? S === A - 1 && (t || "*" === O || I) && (n = E) : (S < e && v(), S === e && !I && (t || "/" === O) && (n = E)), n } e = e.toString(); var x = 0, k = e.length, A = 1, O = null, E = null, S = 0, I = !1, j = [], T = null; return Object.defineProperty({ next: g, peek: v, push: h, skip: function (e, t) { var n = v(); if (n === e) return g(), !0; if (!t) throw o("token '" + n + "', '" + e + "' expected"); return !1 }, cmnt: b }, "line", { get: function () { return A } });/* eslint-enable callback-return */
            } t.exports = o; var i = /[\s{}=;:[\],'"()<>]/g, r = /(?:"([^"\\]*(?:\\.[^"\\]*)*)")/g, s = /(?:'([^'\\]*(?:\\.[^'\\]*)*)')/g, a = /^ *[*/]+ */, d = /^\s*\*?\/*/, p = /\n/g, l = /\s/, u = /\\(.?)/g, y = { 0: "\0", r: "\r", n: "\n", t: "\t" }; o.unescape = n
        }, {}], 35: [function (e, t) {/**
         * Constructs a new reflected message type instance.
         * @classdesc Reflected message type.
         * @extends NamespaceBase
         * @constructor
         * @param {string} name Message name
         * @param {Object.<string,*>} [options] Declared options
         */function n(e, t) {/**
             * Message fields.
             * @type {Object.<string,Field>}
             */ // toJSON, marker
/**
             * Oneofs declared within this namespace, if any.
             * @type {Object.<string,OneOf>}
             */ // toJSON
/**
             * Extension ranges, if any.
             * @type {number[][]}
             */ // toJSON
/**
             * Reserved ranges, if any.
             * @type {Array.<number[]|string>}
             */ // toJSON
/*?
     * Whether this type is a legacy group.
     * @type {boolean|undefined}
     */ // toJSON
/**
             * Cached fields by id.
             * @type {Object.<number,Field>|null}
             * @private
             */ /**
             * Cached fields as an array.
             * @type {Field[]|null}
             * @private
             */ /**
             * Cached oneofs as an array.
             * @type {OneOf[]|null}
             * @private
             */ /**
             * Cached constructor.
             * @type {Constructor<{}>}
             * @private
             */r.call(this, e, t), this.fields = {}, this.oneofs = undefined$1, this.extensions = undefined$1, this.reserved = undefined$1, this.group = undefined$1, this._fieldsById = null, this._fieldsArray = null, this._oneofsArray = null, this._ctor = null
            } function o(e) { return e._fieldsById = e._fieldsArray = e._oneofsArray = null, delete e.encode, delete e.decode, delete e.verify, e }/**
         * Message type descriptor.
         * @interface IType
         * @extends INamespace
         * @property {Object.<string,IOneOf>} [oneofs] Oneof descriptors
         * @property {Object.<string,IField>} fields Field descriptors
         * @property {number[][]} [extensions] Extension ranges
         * @property {number[][]} [reserved] Reserved ranges
         * @property {boolean} [group=false] Whether a legacy group or not
         */ /**
         * Creates a message type from a message type descriptor.
         * @param {string} name Message name
         * @param {IType} json Message type descriptor
         * @returns {Type} Created message type
         */t.exports = n;// extends Namespace
            var r = e(23); ((n.prototype = Object.create(r.prototype)).constructor = n).className = "Type"; var s = e(15), a = e(25), d = e(16), p = e(20), l = e(33), u = e(21), y = e(27), f = e(42), m = e(37), c = e(14), g = e(13), h = e(40), v = e(12), b = e(41);/**
         * Generates a constructor function for the specified type.
         * @param {Type} mtype Message type
         * @returns {Codegen} Codegen instance
         */ /**
         * Converts this message type to a message type descriptor.
         * @param {IToJSONOptions} [toJSONOptions] JSON conversion options
         * @returns {IType} Message type descriptor
         */ /**
         * @override
         */ /**
         * @override
         */ /**
         * Adds a nested object to this type.
         * @param {ReflectionObject} object Nested object to add
         * @returns {Type} `this`
         * @throws {TypeError} If arguments are invalid
         * @throws {Error} If there is already a nested object with this name or, if a field, when there is already a field with this id
         */ /**
         * Removes a nested object from this type.
         * @param {ReflectionObject} object Nested object to remove
         * @returns {Type} `this`
         * @throws {TypeError} If arguments are invalid
         * @throws {Error} If `object` is not a member of this type
         */ /**
         * Tests if the specified id is reserved.
         * @param {number} id Id to test
         * @returns {boolean} `true` if reserved, otherwise `false`
         */ /**
         * Tests if the specified name is reserved.
         * @param {string} name Name to test
         * @returns {boolean} `true` if reserved, otherwise `false`
         */ /**
         * Creates a new message of this type using the specified properties.
         * @param {Object.<string,*>} [properties] Properties to set
         * @returns {Message<{}>} Message instance
         */ /**
         * Sets up {@link Type#encode|encode}, {@link Type#decode|decode} and {@link Type#verify|verify}.
         * @returns {Type} `this`
         */ /**
         * Encodes a message of this type. Does not implicitly {@link Type#verify|verify} messages.
         * @param {Message<{}>|Object.<string,*>} message Message instance or plain object
         * @param {Writer} [writer] Writer to encode to
         * @returns {Writer} writer
         */ /**
         * Encodes a message of this type preceeded by its byte length as a varint. Does not implicitly {@link Type#verify|verify} messages.
         * @param {Message<{}>|Object.<string,*>} message Message instance or plain object
         * @param {Writer} [writer] Writer to encode to
         * @returns {Writer} writer
         */ /**
         * Decodes a message of this type.
         * @param {Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Length of the message, if known beforehand
         * @returns {Message<{}>} Decoded message
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {util.ProtocolError<{}>} If required fields are missing
         */ /**
         * Decodes a message of this type preceeded by its byte length as a varint.
         * @param {Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {Message<{}>} Decoded message
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {util.ProtocolError} If required fields are missing
         */ /**
         * Verifies that field values are valid and that required fields are present.
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {null|string} `null` if valid, otherwise the reason why it is not
         */ /**
         * Creates a new message of this type from a plain object. Also converts values to their respective internal types.
         * @param {Object.<string,*>} object Plain object to convert
         * @returns {Message<{}>} Message instance
         */ /**
         * Conversion options as used by {@link Type#toObject} and {@link Message.toObject}.
         * @interface IConversionOptions
         * @property {Function} [longs] Long conversion type.
         * Valid values are `String` and `Number` (the global types).
         * Defaults to copy the present value, which is a possibly unsafe number without and a {@link Long} with a long library.
         * @property {Function} [enums] Enum value conversion type.
         * Only valid value is `String` (the global type).
         * Defaults to copy the present value, which is the numeric id.
         * @property {Function} [bytes] Bytes value conversion type.
         * Valid values are `Array` and (a base64 encoded) `String` (the global types).
         * Defaults to copy the present value, which usually is a Buffer under node and an Uint8Array in the browser.
         * @property {boolean} [defaults=false] Also sets default values on the resulting object
         * @property {boolean} [arrays=false] Sets empty arrays for missing repeated fields even if `defaults=false`
         * @property {boolean} [objects=false] Sets empty objects for missing map fields even if `defaults=false`
         * @property {boolean} [oneofs=false] Includes virtual oneof properties set to the present field's name, if any
         * @property {boolean} [json=false] Performs additional JSON compatibility conversions, i.e. NaN and Infinity to strings
         */ /**
         * Creates a plain object from a message of this type. Also converts values to other types if specified.
         * @param {Message<{}>} message Message instance
         * @param {IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */ /**
         * Decorator function as returned by {@link Type.d} (TypeScript).
         * @typedef TypeDecorator
         * @type {function}
         * @param {Constructor<T>} target Target constructor
         * @returns {undefined}
         * @template T extends Message<T>
         */ /**
         * Type decorator (TypeScript).
         * @param {string} [typeName] Type name, defaults to the constructor's name
         * @returns {TypeDecorator<T>} Decorator function
         * @template T extends Message<T>
         */Object.defineProperties(n.prototype, {/**
             * Message fields by id.
             * @name Type#fieldsById
             * @type {Object.<number,Field>}
             * @readonly
             */fieldsById: { get: function () {/* istanbul ignore if */if (this._fieldsById) return this._fieldsById; this._fieldsById = {}; for (var e = Object.keys(this.fields), t = 0; t < e.length; ++t) { var n = this.fields[e[t]], o = n.id;/* istanbul ignore if */if (this._fieldsById[o]) throw Error("duplicate id " + o + " in " + this); this._fieldsById[o] = n } return this._fieldsById } },/**
             * Fields of this message as an array for iteration.
             * @name Type#fieldsArray
             * @type {Field[]}
             * @readonly
             */fieldsArray: { get: function () { return this._fieldsArray || (this._fieldsArray = m.toArray(this.fields)) } },/**
             * Oneofs of this message as an array for iteration.
             * @name Type#oneofsArray
             * @type {OneOf[]}
             * @readonly
             */oneofsArray: { get: function () { return this._oneofsArray || (this._oneofsArray = m.toArray(this.oneofs)) } },/**
             * The registered constructor, if any registered, otherwise a generic constructor.
             * Assigning a function replaces the internal constructor. If the function does not extend {@link Message} yet, its prototype will be setup accordingly and static methods will be populated. If it already extends {@link Message}, it will just replace the internal constructor.
             * @name Type#ctor
             * @type {Constructor<{}>}
             */ctor: {
                    get: function () { return this._ctor || (this.ctor = n.generateConstructor(this)()) }, set: function (e) {// Ensure proper prototype
                        var t = e.prototype; t instanceof u || ((e.prototype = new u).constructor = e, m.merge(e.prototype, t)), e.$type = e.prototype.$type = this, m.merge(e, u, !0), this._ctor = e; for (// Messages have non-enumerable default values on their prototype
                            var n = 0; n </* initializes */this.fieldsArray.length; ++n)this._fieldsArray[n].resolve();// ensures a proper value
                        // Messages have non-enumerable getters and setters for each virtual oneof field
                        var o = {}; for (n = 0; n </* initializes */this.oneofsArray.length; ++n)o[this._oneofsArray[n].resolve().name] = { get: m.oneOfGetter(this._oneofsArray[n].oneof), set: m.oneOfSetter(this._oneofsArray[n].oneof) }; n && Object.defineProperties(e.prototype, o)
                    }
                }
            }), n.generateConstructor = function (e) {/* eslint-disable no-unexpected-multiline */ // explicitly initialize mutable object/array fields so that these aren't just inherited from the prototype
                for (var t = m.codegen(["p"], e.name), n = 0, o; n < e.fieldsArray.length; ++n)(o = e._fieldsArray[n]).map ? t("this%s={}", m.safeProp(o.name)) : o.repeated && t("this%s=[]", m.safeProp(o.name)); return t("if(p)for(var ks=Object.keys(p),i=0;i<ks.length;++i)if(p[ks[i]]!=null)")// omit undefined or null
                    ("this[ks[i]]=p[ks[i]]");/* eslint-enable no-unexpected-multiline */
            }, n.fromJSON = function (e, t) {
                var o = new n(e, t.options); o.extensions = t.extensions, o.reserved = t.reserved; for (var u = Object.keys(t.fields), y = 0; y < u.length; ++y)o.add(("undefined" == typeof t.fields[u[y]].keyType ? d.fromJSON : p.fromJSON)(u[y], t.fields[u[y]])); if (t.oneofs) for (u = Object.keys(t.oneofs), y = 0; y < u.length; ++y)o.add(a.fromJSON(u[y], t.oneofs[u[y]])); if (t.nested) for (u = Object.keys(t.nested), y = 0; y < u.length; ++y) {
                    var f = t.nested[u[y]]; o.add(// most to least likely
                        (f.id === undefined$1 ? f.fields === undefined$1 ? f.values === undefined$1 ? f.methods === undefined$1 ? r.fromJSON : l.fromJSON : s.fromJSON : n.fromJSON : d.fromJSON)(u[y], f))
                } return t.extensions && t.extensions.length && (o.extensions = t.extensions), t.reserved && t.reserved.length && (o.reserved = t.reserved), t.group && (o.group = !0), t.comment && (o.comment = t.comment), o
            }, n.prototype.toJSON = function (e) { var t = r.prototype.toJSON.call(this, e), n = !!e && !!e.keepComments; return m.toObject(["options", t && t.options || undefined$1, "oneofs", r.arrayToJSON(this.oneofsArray, e), "fields", r.arrayToJSON(this.fieldsArray.filter(function (e) { return !e.declaringField }), e) || {}, "extensions", this.extensions && this.extensions.length ? this.extensions : undefined$1, "reserved", this.reserved && this.reserved.length ? this.reserved : undefined$1, "group", this.group || undefined$1, "nested", t && t.nested || undefined$1, "comment", n ? this.comment : undefined$1]) }, n.prototype.resolveAll = function () { for (var e = this.fieldsArray, t = 0; t < e.length;)e[t++].resolve(); var n = this.oneofsArray; for (t = 0; t < n.length;)n[t++].resolve(); return r.prototype.resolveAll.call(this) }, n.prototype.get = function (e) { return this.fields[e] || this.oneofs && this.oneofs[e] || this.nested && this.nested[e] || null }, n.prototype.add = function (e) {
                if (this.get(e.name)) throw Error("duplicate name '" + e.name + "' in " + this); if (e instanceof d && e.extend === undefined$1) {// NOTE: Extension fields aren't actual fields on the declaring type, but nested objects.
                    // The root object takes care of adding distinct sister-fields to the respective extended
                    // type instead.
                    // avoids calling the getter if not absolutely necessary because it's called quite frequently
                    if (this._fieldsById ?/* istanbul ignore next */this._fieldsById[e.id] : this.fieldsById[e.id]) throw Error("duplicate id " + e.id + " in " + this); if (this.isReservedId(e.id)) throw Error("id " + e.id + " is reserved in " + this); if (this.isReservedName(e.name)) throw Error("name '" + e.name + "' is reserved in " + this); return e.parent && e.parent.remove(e), this.fields[e.name] = e, e.message = this, e.onAdd(this), o(this)
                } return e instanceof a ? (this.oneofs || (this.oneofs = {}), this.oneofs[e.name] = e, e.onAdd(this), o(this)) : r.prototype.add.call(this, e)
            }, n.prototype.remove = function (e) {
                if (e instanceof d && e.extend === undefined$1) {// See Type#add for the reason why extension fields are excluded here.
/* istanbul ignore if */if (!this.fields || this.fields[e.name] !== e) throw Error(e + " is not a member of " + this); return delete this.fields[e.name], e.parent = null, e.onRemove(this), o(this)
                } if (e instanceof a) {/* istanbul ignore if */if (!this.oneofs || this.oneofs[e.name] !== e) throw Error(e + " is not a member of " + this); return delete this.oneofs[e.name], e.parent = null, e.onRemove(this), o(this) } return r.prototype.remove.call(this, e)
            }, n.prototype.isReservedId = function (e) { return r.isReservedId(this.reserved, e) }, n.prototype.isReservedName = function (e) { return r.isReservedName(this.reserved, e) }, n.prototype.create = function (e) { return new this.ctor(e) }, n.prototype.setup = function () {// Sets up everything at once so that the prototype chain does not have to be re-evaluated
                // multiple times (V8, soft-deopt prototype-check).
                for (var e = this.fullName, t = [], n = 0; n </* initializes */this.fieldsArray.length; ++n)t.push(this._fieldsArray[n].resolve().resolvedType);// Replace setup methods with type-specific generated functions
                this.encode = c(this)({ Writer: f, types: t, util: m }), this.decode = g(this)({ Reader: y, types: t, util: m }), this.verify = h(this)({ types: t, util: m }), this.fromObject = v.fromObject(this)({ types: t, util: m }), this.toObject = v.toObject(this)({ types: t, util: m });// Inject custom wrappers for common types
                var o = b[e]; if (o) {
                    var r = Object.create(this);// if (wrapper.fromObject) {
                    // }
                    // if (wrapper.toObject) {
                    r.fromObject = this.fromObject, this.fromObject = o.fromObject.bind(r), r.toObject = this.toObject, this.toObject = o.toObject.bind(r)
                } return this
            }, n.prototype.encode = function (e, t) {
                return this.setup().encode(e, t);// overrides this method
            }, n.prototype.encodeDelimited = function (e, t) { return this.encode(e, t && t.len ? t.fork() : t).ldelim() }, n.prototype.decode = function (e, t) {
                return this.setup().decode(e, t);// overrides this method
            }, n.prototype.decodeDelimited = function (e) { return e instanceof y || (e = y.create(e)), this.decode(e, e.uint32()) }, n.prototype.verify = function (e) {
                return this.setup().verify(e);// overrides this method
            }, n.prototype.fromObject = function (e) { return this.setup().fromObject(e) }, n.prototype.toObject = function (e, t) { return this.setup().toObject(e, t) }, n.d = function (e) { return function (t) { m.decorateType(t, e) } }
        }, { 12: 12, 13: 13, 14: 14, 15: 15, 16: 16, 20: 20, 21: 21, 23: 23, 25: 25, 27: 27, 33: 33, 37: 37, 40: 40, 41: 41, 42: 42 }], 36: [function (e, t, n) {
            function o(e, t) { var n = 0, r = {}; for (t |= 0; n < e.length;)r[a[n + t]] = e[n++]; return r }/**
         * Basic type wire types.
         * @type {Object.<string,number>}
         * @const
         * @property {number} double=1 Fixed64 wire type
         * @property {number} float=5 Fixed32 wire type
         * @property {number} int32=0 Varint wire type
         * @property {number} uint32=0 Varint wire type
         * @property {number} sint32=0 Varint wire type
         * @property {number} fixed32=5 Fixed32 wire type
         * @property {number} sfixed32=5 Fixed32 wire type
         * @property {number} int64=0 Varint wire type
         * @property {number} uint64=0 Varint wire type
         * @property {number} sint64=0 Varint wire type
         * @property {number} fixed64=1 Fixed64 wire type
         * @property {number} sfixed64=1 Fixed64 wire type
         * @property {number} bool=0 Varint wire type
         * @property {number} string=2 Ldelim wire type
         * @property {number} bytes=2 Ldelim wire type
         */ /**
         * Common type constants.
         * @namespace
         */var i = n, r = e(37), a = ["double",// 0
                "float",// 1
                "int32",// 2
                "uint32",// 3
                "sint32",// 4
                "fixed32",// 5
                "sfixed32",// 6
                "int64",// 7
                "uint64",// 8
                "sint64",// 9
                "fixed64",// 10
                "sfixed64",// 11
                "bool",// 12
                "string",// 13
                "bytes"// 14
            ];/**
         * Basic type defaults.
         * @type {Object.<string,*>}
         * @const
         * @property {number} double=0 Double default
         * @property {number} float=0 Float default
         * @property {number} int32=0 Int32 default
         * @property {number} uint32=0 Uint32 default
         * @property {number} sint32=0 Sint32 default
         * @property {number} fixed32=0 Fixed32 default
         * @property {number} sfixed32=0 Sfixed32 default
         * @property {number} int64=0 Int64 default
         * @property {number} uint64=0 Uint64 default
         * @property {number} sint64=0 Sint32 default
         * @property {number} fixed64=0 Fixed64 default
         * @property {number} sfixed64=0 Sfixed64 default
         * @property {boolean} bool=false Bool default
         * @property {string} string="" String default
         * @property {Array.<number>} bytes=Array(0) Bytes default
         * @property {null} message=null Message default
         */ /**
         * Basic long type wire types.
         * @type {Object.<string,number>}
         * @const
         * @property {number} int64=0 Varint wire type
         * @property {number} uint64=0 Varint wire type
         * @property {number} sint64=0 Varint wire type
         * @property {number} fixed64=1 Fixed64 wire type
         * @property {number} sfixed64=1 Fixed64 wire type
         */ /**
         * Allowed types for map keys with their associated wire type.
         * @type {Object.<string,number>}
         * @const
         * @property {number} int32=0 Varint wire type
         * @property {number} uint32=0 Varint wire type
         * @property {number} sint32=0 Varint wire type
         * @property {number} fixed32=5 Fixed32 wire type
         * @property {number} sfixed32=5 Fixed32 wire type
         * @property {number} int64=0 Varint wire type
         * @property {number} uint64=0 Varint wire type
         * @property {number} sint64=0 Varint wire type
         * @property {number} fixed64=1 Fixed64 wire type
         * @property {number} sfixed64=1 Fixed64 wire type
         * @property {number} bool=0 Varint wire type
         * @property {number} string=2 Ldelim wire type
         */ /**
         * Allowed types for packed repeated fields with their associated wire type.
         * @type {Object.<string,number>}
         * @const
         * @property {number} double=1 Fixed64 wire type
         * @property {number} float=5 Fixed32 wire type
         * @property {number} int32=0 Varint wire type
         * @property {number} uint32=0 Varint wire type
         * @property {number} sint32=0 Varint wire type
         * @property {number} fixed32=5 Fixed32 wire type
         * @property {number} sfixed32=5 Fixed32 wire type
         * @property {number} int64=0 Varint wire type
         * @property {number} uint64=0 Varint wire type
         * @property {number} sint64=0 Varint wire type
         * @property {number} fixed64=1 Fixed64 wire type
         * @property {number} sfixed64=1 Fixed64 wire type
         * @property {number} bool=0 Varint wire type
         */i.basic = o([/* double   */1,/* float    */5,/* int32    */0,/* uint32   */0,/* sint32   */0,/* fixed32  */5,/* sfixed32 */5,/* int64    */0,/* uint64   */0,/* sint64   */0,/* fixed64  */1,/* sfixed64 */1,/* bool     */0,/* string   */2,/* bytes    */2]), i.defaults = o([/* double   */0,/* float    */0,/* int32    */0,/* uint32   */0,/* sint32   */0,/* fixed32  */0,/* sfixed32 */0,/* int64    */0,/* uint64   */0,/* sint64   */0,/* fixed64  */0,/* sfixed64 */0, !1,/* string   */"",/* bytes    */r.emptyArray,/* message  */null]), i.long = o([/* int64    */0,/* uint64   */0,/* sint64   */0,/* fixed64  */1,/* sfixed64 */1], 7), i.mapKey = o([/* int32    */0,/* uint32   */0,/* sint32   */0,/* fixed32  */5,/* sfixed32 */5,/* int64    */0,/* uint64   */0,/* sint64   */0,/* fixed64  */1,/* sfixed64 */1,/* bool     */0,/* string   */2], 2), i.packed = o([/* double   */1,/* float    */5,/* int32    */0,/* uint32   */0,/* sint32   */0,/* fixed32  */5,/* sfixed32 */5,/* int64    */0,/* uint64   */0,/* sint64   */0,/* fixed64  */1,/* sfixed64 */1,/* bool     */0])
        }, { 37: 37 }], 37: [function (e, t) {/**
         * Various utility functions.
         * @namespace
         */var n = t.exports = e(39), o = e(30), i,// cyclic
                r; n.codegen = e(3), n.fetch = e(5), n.path = e(8), n.fs = n.inquire("fs"), n.toArray = function (e) { if (e) { for (var t = Object.keys(e), n = Array(t.length), o = 0; o < t.length;)n[o] = e[t[o++]]; return n } return [] }, n.toObject = function (e) { for (var t = {}, n = 0; n < e.length;) { var o = e[n++], i = e[n++]; i !== undefined$1 && (t[o] = i) } return t }; var s = /\\/g, a = /"/g;/**
         * Tests whether the specified name is a reserved word in JS.
         * @param {string} name Name to test
         * @returns {boolean} `true` if reserved, otherwise `false`
         */n.isReserved = function (e) { return /^(?:do|if|in|for|let|new|try|var|case|else|enum|eval|false|null|this|true|void|with|break|catch|class|const|super|throw|while|yield|delete|export|import|public|return|static|switch|typeof|default|extends|finally|package|private|continue|debugger|function|arguments|interface|protected|implements|instanceof)$/.test(e) }, n.safeProp = function (e) { return !/^[$\w_]+$/.test(e) || n.isReserved(e) ? "[\"" + e.replace(s, "\\\\").replace(a, "\\\"") + "\"]" : "." + e }, n.ucFirst = function (e) { return e.charAt(0).toUpperCase() + e.substring(1) }; var d = /_([a-z])/g;/**
         * Converts a string to camel case.
         * @param {string} str String to convert
         * @returns {string} Converted string
         */n.camelCase = function (e) { return e.substring(0, 1) + e.substring(1).replace(d, function (e, t) { return t.toUpperCase() }) }, n.compareFieldsById = function (e, t) { return e.id - t.id }, n.decorateType = function (t, o) {/* istanbul ignore if */if (t.$type) return o && t.$type.name !== o && (n.decorateRoot.remove(t.$type), t.$type.name = o, n.decorateRoot.add(t.$type)), t.$type;/* istanbul ignore next */i || (i = e(35)); var r = new i(o || t.name); return n.decorateRoot.add(r), r.ctor = t, Object.defineProperty(t, "$type", { value: r, enumerable: !1 }), Object.defineProperty(t.prototype, "$type", { value: r, enumerable: !1 }), r }; var p = 0;/**
         * Decorator helper for enums (TypeScript).
         * @param {Object} object Enum object
         * @returns {Enum} Reflected enum
         */ /**
         * Decorator root (TypeScript).
         * @name util.decorateRoot
         * @type {Root}
         * @readonly
         */n.decorateEnum = function (t) {/* istanbul ignore if */if (t.$type) return t.$type;/* istanbul ignore next */r || (r = e(15)); var o = new r("Enum" + p++, t); return n.decorateRoot.add(o), Object.defineProperty(t, "$type", { value: o, enumerable: !1 }), o }, Object.defineProperty(n, "decorateRoot", { get: function () { return o.decorated || (o.decorated = new (e(29))) } })
        }, { 15: 15, 29: 29, 3: 3, 30: 30, 35: 35, 39: 39, 5: 5, 8: 8 }], 38: [function (e, t) {/**
         * Constructs new long bits.
         * @classdesc Helper class for working with the low and high bits of a 64 bit value.
         * @memberof util
         * @constructor
         * @param {number} lo Low 32 bits, unsigned
         * @param {number} hi High 32 bits, unsigned
         */function n(e, t) {// note that the casts below are theoretically unnecessary as of today, but older statically
// generated converter code might still call the ctor with signed 32bits. kept for compat.
/**
             * Low bits.
             * @type {number}
             */ /**
             * High bits.
             * @type {number}
             */this.lo = e >>> 0, this.hi = t >>> 0
            }/**
         * Zero bits.
         * @memberof util.LongBits
         * @type {util.LongBits}
         */t.exports = n; var o = e(39), i = n.zero = new n(0, 0); i.toNumber = function () { return 0 }, i.zzEncode = i.zzDecode = function () { return this }, i.length = function () { return 1 };/**
         * Zero hash.
         * @memberof util.LongBits
         * @type {string}
         */var r = n.zeroHash = "\0\0\0\0\0\0\0\0";/**
         * Constructs new long bits from the specified number.
         * @param {number} value Value
         * @returns {util.LongBits} Instance
         */n.fromNumber = function (e) { if (0 === e) return i; var t = 0 > e; t && (e = -e); var o = e >>> 0, r = (e - o) / 4294967296 >>> 0; return t && (r = ~r >>> 0, o = ~o >>> 0, 4294967295 < ++o && (o = 0, 4294967295 < ++r && (r = 0))), new n(o, r) }, n.from = function (e) { if ("number" == typeof e) return n.fromNumber(e); if (o.isString(e))/* istanbul ignore else */if (o.Long) e = o.Long.fromString(e); else return n.fromNumber(parseInt(e, 10)); return e.low || e.high ? new n(e.low >>> 0, e.high >>> 0) : i }, n.prototype.toNumber = function (e) { if (!e && this.hi >>> 31) { var t = ~this.lo + 1 >>> 0, n = ~this.hi >>> 0; return t || (n = n + 1 >>> 0), -(t + 4294967296 * n) } return this.lo + 4294967296 * this.hi }, n.prototype.toLong = function (e) { return o.Long ? new o.Long(0 | this.lo, 0 | this.hi, !!e)/* istanbul ignore next */ : { low: 0 | this.lo, high: 0 | this.hi, unsigned: !!e } }; var s = String.prototype.charCodeAt;/**
         * Constructs new long bits from the specified 8 characters long hash.
         * @param {string} hash Hash
         * @returns {util.LongBits} Bits
         */ /**
         * Converts this long bits to a 8 characters long hash.
         * @returns {string} Hash
         */ /**
         * Zig-zag encodes this long bits.
         * @returns {util.LongBits} `this`
         */ /**
         * Zig-zag decodes this long bits.
         * @returns {util.LongBits} `this`
         */ /**
         * Calculates the length of this longbits when encoded as a varint.
         * @returns {number} Length
         */n.fromHash = function (e) { return e === r ? i : new n((s.call(e, 0) | s.call(e, 1) << 8 | s.call(e, 2) << 16 | s.call(e, 3) << 24) >>> 0, (s.call(e, 4) | s.call(e, 5) << 8 | s.call(e, 6) << 16 | s.call(e, 7) << 24) >>> 0) }, n.prototype.toHash = function () { return _StringfromCharCode(255 & this.lo, 255 & this.lo >>> 8, 255 & this.lo >>> 16, this.lo >>> 24, 255 & this.hi, 255 & this.hi >>> 8, 255 & this.hi >>> 16, this.hi >>> 24) }, n.prototype.zzEncode = function () { var e = this.hi >> 31; return this.hi = ((this.hi << 1 | this.lo >>> 31) ^ e) >>> 0, this.lo = (this.lo << 1 ^ e) >>> 0, this }, n.prototype.zzDecode = function () { var e = -(1 & this.lo); return this.lo = ((this.lo >>> 1 | this.hi << 31) ^ e) >>> 0, this.hi = (this.hi >>> 1 ^ e) >>> 0, this }, n.prototype.length = function () { var e = this.lo, t = (this.lo >>> 28 | this.hi << 4) >>> 0, n = this.hi >>> 24; return 0 == n ? 0 == t ? 16384 > e ? 128 > e ? 1 : 2 : 2097152 > e ? 3 : 4 : 16384 > t ? 128 > t ? 5 : 6 : 2097152 > t ? 7 : 8 : 128 > n ? 9 : 10 }
        }, { 39: 39 }], 39: [function (e, t, n) {/**
         * Merges the properties of the source object into the destination object.
         * @memberof util
         * @param {Object.<string,*>} dst Destination object
         * @param {Object.<string,*>} src Source object
         * @param {boolean} [ifNotSet=false] Merges only if the key is not already set
         * @returns {Object.<string,*>} Destination object
         */function o(e, t, n) {// used by converters
                for (var o = Object.keys(t), r = 0; r < o.length; ++r)e[o[r]] !== undefined$1 && n || (e[o[r]] = t[o[r]]); return e
            }/**
         * Creates a custom error constructor.
         * @memberof util
         * @param {string} name Error name
         * @returns {Constructor<Error>} Custom error constructor
         */function i(e) {
                function t(e, n) {
                    return this instanceof t ? void (// Error.call(this, message);
                        // ^ just returns a new error instance because the ctor can be called as a function
                        Object.defineProperty(this, "message", { get: function () { return e } }), Error.captureStackTrace ?// node
                            Error.captureStackTrace(this, t) : Object.defineProperty(this, "stack", { value: new Error().stack || "" }), n && o(this, n)) : new t(e, n)
                } return (t.prototype = Object.create(Error.prototype)).constructor = t, Object.defineProperty(t.prototype, "name", { get: function () { return e } }), t.prototype.toString = function () { return this.name + ": " + this.message }, t
            } var r = n;// used to return a Promise where callback is omitted
            // converts to / from base64 encoded strings
            // base class of rpc.Service
            // float handling accross browsers
            // requires modules optionally and hides the call from bundlers
            // converts to / from utf8 encoded strings
            // provides a node-like buffer pool in the browser
            // utility to work with the low and high bits of a 64 bit value
            // global object reference
            // eslint-disable-line no-invalid-this
            /**
                     * An immuable empty array.
                     * @memberof util
                     * @type {Array.<*>}
                     * @const
                     */ // used on prototypes
            /**
                     * An immutable empty object.
                     * @type {Object}
                     * @const
                     */ // used on prototypes
            /**
                     * Whether running within node or not.
                     * @memberof util
                     * @type {boolean}
                     * @const
                     */ /**
            * Tests if the specified value is an integer.
            * @function
            * @param {*} value Value to test
            * @returns {boolean} `true` if the value is an integer
            */ /**
            * Tests if the specified value is a string.
            * @param {*} value Value to test
            * @returns {boolean} `true` if the value is a string
            */ /**
            * Tests if the specified value is a non-null object.
            * @param {*} value Value to test
            * @returns {boolean} `true` if the value is a non-null object
            */ /**
            * Checks if a property on a message is considered to be present.
            * This is an alias of {@link util.isSet}.
            * @function
            * @param {Object} obj Plain object or message instance
            * @param {string} prop Property name
            * @returns {boolean} `true` if considered to be present, otherwise `false`
            */ /**
            * Any compatible Buffer instance.
            * This is a minimal stand-alone definition of a Buffer instance. The actual type is that exported by node's typings.
            * @interface Buffer
            * @extends Uint8Array
            */ /**
            * Node's Buffer class if available.
            * @type {Constructor<Buffer>}
            */ // Internal alias of or polyfull for Buffer.from.
            // Internal alias of or polyfill for Buffer.allocUnsafe.
            /**
                     * Creates a new buffer of whatever type supported by the environment.
                     * @param {number|number[]} [sizeOrArray=0] Buffer size or number array
                     * @returns {Uint8Array|Buffer} Buffer
                     */ /**
            * Array implementation used in the browser. `Uint8Array` if supported, otherwise `Array`.
            * @type {Constructor<Uint8Array>}
            */ /**
            * Any compatible Long instance.
            * This is a minimal stand-alone definition of a Long instance. The actual type is that exported by long.js.
            * @interface Long
            * @property {number} low Low bits
            * @property {number} high High bits
            * @property {boolean} unsigned Whether unsigned or not
            */ /**
            * Long.js's Long class if available.
            * @type {Constructor<Long>}
            */ /**
            * Regular expression used to verify 2 bit (`bool`) map keys.
            * @type {RegExp}
            * @const
            */ /**
            * Regular expression used to verify 32 bit (`int32` etc.) map keys.
            * @type {RegExp}
            * @const
            */ /**
            * Regular expression used to verify 64 bit (`int64` etc.) map keys.
            * @type {RegExp}
            * @const
            */ /**
            * Converts a number or long to an 8 characters long hash string.
            * @param {Long|number} value Value to convert
            * @returns {string} Hash
            */ /**
            * Converts an 8 characters long hash string to a long or number.
            * @param {string} hash Hash
            * @param {boolean} [unsigned=false] Whether unsigned or not
            * @returns {Long|number} Original value
            */ /**
            * Converts the first character of a string to lower case.
            * @param {string} str String to convert
            * @returns {string} Converted string
            */ /**
            * Constructs a new protocol error.
            * @classdesc Error subclass indicating a protocol specifc error.
            * @memberof util
            * @extends Error
            * @template T extends Message<T>
            * @constructor
            * @param {string} message Error message
            * @param {Object.<string,*>} [properties] Additional properties
            * @example
            * try {
            *     MyMessage.decode(someBuffer); // throws if required fields are missing
            * } catch (e) {
            *     if (e instanceof ProtocolError && e.instance)
            *         console.log("decoded so far: " + JSON.stringify(e.instance));
            * }
            */ /**
            * So far decoded message instance.
            * @name util.ProtocolError#instance
            * @type {Message<T>}
            */ /**
            * A OneOf getter as returned by {@link util.oneOfGetter}.
            * @typedef OneOfGetter
            * @type {function}
            * @returns {string|undefined} Set field name, if any
            */ /**
            * Builds a getter for a oneof's present field name.
            * @param {string[]} fieldNames Field names
            * @returns {OneOfGetter} Unbound getter
            */ /**
            * A OneOf setter as returned by {@link util.oneOfSetter}.
            * @typedef OneOfSetter
            * @type {function}
            * @param {string|undefined} value Field name
            * @returns {undefined}
            */ /**
            * Builds a setter for a oneof's present field name.
            * @param {string[]} fieldNames Field names
            * @returns {OneOfSetter} Unbound setter
            */ /**
            * Default conversion options used for {@link Message#toJSON} implementations.
            *
            * These options are close to proto3's JSON mapping with the exception that internal types like Any are handled just like messages. More precisely:
            *
            * - Longs become strings
            * - Enums become string keys
            * - Bytes become base64 encoded strings
            * - (Sub-)Messages become plain objects
            * - Maps become plain objects with all string keys
            * - Repeated fields become arrays
            * - NaN and Infinity for float and double fields become strings
            *
            * @type {IConversionOptions}
            * @see https://developers.google.com/protocol-buffers/docs/proto3?hl=en#json
            */ // Sets up buffer utility according to the environment (called in index-minimal)
            r.asPromise = e(1), r.base64 = e(2), r.EventEmitter = e(4), r.float = e(6), r.inquire = e(7), r.utf8 = e(10), r.pool = e(9), r.LongBits = e(38), r.global = "undefined" != typeof window && window || "undefined" != typeof global && global || "undefined" != typeof self && self || this, r.emptyArray = Object.freeze ? Object.freeze([]) :/* istanbul ignore next */[], r.emptyObject = Object.freeze ? Object.freeze({}) :/* istanbul ignore next */{}, r.isNode = !!(r.global.process && r.global.process.versions && r.global.process.versions.node), r.isInteger = Number.isInteger ||/* istanbul ignore next */function (e) { return "number" == typeof e && isFinite(e) && _Mathfloor(e) === e }, r.isString = function (e) { return "string" == typeof e || e instanceof String }, r.isObject = function (e) { return e && "object" == typeof e }, r.isset =/**
             * Checks if a property on a message is considered to be present.
             * @param {Object} obj Plain object or message instance
             * @param {string} prop Property name
             * @returns {boolean} `true` if considered to be present, otherwise `false`
             */r.isSet = function (e, t) { var n = e[t]; return !!(null != n && e.hasOwnProperty(t)) && ("object" != typeof n || 0 < (Array.isArray(n) ? n.length : Object.keys(n).length)) }, r.Buffer = function () {
                    try {
                        var e = r.inquire("buffer").Buffer;// refuse to use non-node buffers if not explicitly assigned (perf reasons):
                        return e.prototype.utf8Write ? e :/* istanbul ignore next */null
                    } catch (t) {/* istanbul ignore next */return null }
                }(), r._Buffer_from = null, r._Buffer_allocUnsafe = null, r.newBuffer = function (e) {/* istanbul ignore next */return "number" == typeof e ? r.Buffer ? r._Buffer_allocUnsafe(e) : new r.Array(e) : r.Buffer ? r._Buffer_from(e) : "undefined" == typeof Uint8Array ? e : new Uint8Array(e) }, r.Array = "undefined" == typeof Uint8Array ?/* istanbul ignore next */Array : Uint8Array, r.Long =/* istanbul ignore next */r.global.dcodeIO &&/* istanbul ignore next */r.global.dcodeIO.Long ||/* istanbul ignore next */r.global.Long || r.inquire("long"), r.key2Re = /^true|false|0|1$/, r.key32Re = /^-?(?:0|[1-9][0-9]*)$/, r.key64Re = /^(?:[\\x00-\\xff]{8}|-?(?:0|[1-9][0-9]*))$/, r.longToHash = function (e) { return e ? r.LongBits.from(e).toHash() : r.LongBits.zeroHash }, r.longFromHash = function (e, t) { var n = r.LongBits.fromHash(e); return r.Long ? r.Long.fromBits(n.lo, n.hi, t) : n.toNumber(!!t) }, r.merge = o, r.lcFirst = function (e) { return e.charAt(0).toLowerCase() + e.substring(1) }, r.newError = i, r.ProtocolError = i("ProtocolError"), r.oneOfGetter = function (e) {
                    for (var t = {}, n = 0; n < e.length; ++n)t[e[n]] = 1;/**
             * @returns {string|undefined} Set field name, if any
             * @this Object
             * @ignore
             */return function () {// eslint-disable-line consistent-return
                        for (var e = Object.keys(this), n = e.length - 1; -1 < n; --n)if (1 === t[e[n]] && this[e[n]] !== undefined$1 && null !== this[e[n]]) return e[n]
                    }
                }, r.oneOfSetter = function (e) {/**
             * @param {string} name Field name
             * @returns {undefined}
             * @this Object
             * @ignore
             */return function (t) { for (var n = 0; n < e.length; ++n)e[n] !== t && delete this[e[n]] }
                }, r.toJSONOptions = { longs: String, enums: String, bytes: String, json: !0 }, r._configure = function () {
                    var e = r.Buffer;/* istanbul ignore if */return e ? void (// because node 4.x buffers are incompatible & immutable
                        // see: https://github.com/dcodeIO/protobuf.js/pull/665
                        r._Buffer_from = e.from !== Uint8Array.from && e.from ||/* istanbul ignore next */function (t, n) { return new e(t, n) }, r._Buffer_allocUnsafe = e.allocUnsafe ||/* istanbul ignore next */function (t) { return new e(t) }) : void (r._Buffer_from = r._Buffer_allocUnsafe = null)
                }
        }, { 1: 1, 10: 10, 2: 2, 38: 38, 4: 4, 6: 6, 7: 7, 9: 9 }], 40: [function (e, t) {
            function n(e, t) { return e.name + ": " + t + (e.repeated && "array" !== t ? "[]" : e.map && "object" !== t ? "{k:" + e.keyType + "}" : "") + " expected" }/**
         * Generates a partial value verifier.
         * @param {Codegen} gen Codegen instance
         * @param {Field} field Reflected field
         * @param {number} fieldIndex Field index
         * @param {string} ref Variable reference
         * @returns {Codegen} Codegen instance
         * @ignore
         */function o(e, t, o, i) {/* eslint-disable no-unexpected-multiline */if (!t.resolvedType) switch (t.type) { case "int32": case "uint32": case "sint32": case "fixed32": case "sfixed32": e("if(!util.isInteger(%s))", i)("return%j", n(t, "integer")); break; case "int64": case "uint64": case "sint64": case "fixed64": case "sfixed64": e("if(!util.isInteger(%s)&&!(%s&&util.isInteger(%s.low)&&util.isInteger(%s.high)))", i, i, i, i)("return%j", n(t, "integer|Long")); break; case "float": case "double": e("if(typeof %s!==\"number\")", i)("return%j", n(t, "number")); break; case "bool": e("if(typeof %s!==\"boolean\")", i)("return%j", n(t, "boolean")); break; case "string": e("if(!util.isString(%s))", i)("return%j", n(t, "string")); break; case "bytes": e("if(!(%s&&typeof %s.length===\"number\"||util.isString(%s)))", i, i, i)("return%j", n(t, "buffer")); } else if (t.resolvedType instanceof s) { e("switch(%s){", i)("default:")("return%j", n(t, "enum value")); for (var r = Object.keys(t.resolvedType.values), a = 0; a < r.length; ++a)e("case %i:", t.resolvedType.values[r[a]]); e("break")("}") } else e("{")("var e=types[%i].verify(%s);", o, i)("if(e)")("return%j+e", t.name + ".")("}"); return e;/* eslint-enable no-unexpected-multiline */ }/**
         * Generates a partial key verifier.
         * @param {Codegen} gen Codegen instance
         * @param {Field} field Reflected field
         * @param {string} ref Variable reference
         * @returns {Codegen} Codegen instance
         * @ignore
         */function r(e, t, o) {/* eslint-disable no-unexpected-multiline */switch (t.keyType) {
                case "int32": case "uint32": case "sint32": case "fixed32": case "sfixed32": e("if(!util.key32Re.test(%s))", o)("return%j", n(t, "integer key")); break; case "int64": case "uint64": case "sint64": case "fixed64": case "sfixed64": e("if(!util.key64Re.test(%s))", o)// see comment above: x is ok, d is not
                    ("return%j", n(t, "integer|Long key")); break; case "bool": e("if(!util.key2Re.test(%s))", o)("return%j", n(t, "boolean key"));
            }return e;/* eslint-enable no-unexpected-multiline */
            }/**
         * Generates a verifier specific to the specified message type.
         * @param {Type} mtype Message type
         * @returns {Codegen} Codegen instance
         */t.exports = function (e) {/* eslint-disable no-unexpected-multiline */var t = a.codegen(["m"], e.name + "$verify")("if(typeof m!==\"object\"||m===null)")("return%j", "object expected"), s = e.oneofsArray, d = {}; s.length && t("var p={}"); for (var p = 0; p </* initializes */e.fieldsArray.length; ++p) {
                var l = e._fieldsArray[p].resolve(), u = "m" + a.safeProp(l.name);// !== undefined && !== null
                // map fields
                if (l.optional && t("if(%s!=null&&m.hasOwnProperty(%j)){", u, l.name), l.map) t("if(!util.isObject(%s))", u)("return%j", n(l, "object"))("var k=Object.keys(%s)", u)("for(var i=0;i<k.length;++i){"), r(t, l, "k[i]"), o(t, l, p, u + "[k[i]]")("}"); else if (l.repeated) t("if(!Array.isArray(%s))", u)("return%j", n(l, "array"))("for(var i=0;i<%s.length;++i){", u), o(t, l, p, u + "[i]")("}"); else { if (l.partOf) { var y = a.safeProp(l.partOf.name); 1 === d[l.partOf.name] && t("if(p%s===1)", y)("return%j", l.partOf.name + ": multiple values"), d[l.partOf.name] = 1, t("p%s=1", y) } o(t, l, p, u) } l.optional && t("}")
            } return t("return null");/* eslint-enable no-unexpected-multiline */
            }; var s = e(15), a = e(37)
        }, { 15: 15, 37: 37 }], 41: [function (e, t, n) {/**
         * Wrappers for common types.
         * @type {Object.<string,IWrapper>}
         * @const
         */var o = e(21);/**
         * From object converter part of an {@link IWrapper}.
         * @typedef WrapperFromObjectConverter
         * @type {function}
         * @param {Object.<string,*>} object Plain object
         * @returns {Message<{}>} Message instance
         * @this Type
         */ /**
         * To object converter part of an {@link IWrapper}.
         * @typedef WrapperToObjectConverter
         * @type {function}
         * @param {Message<{}>} message Message instance
         * @param {IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         * @this Type
         */ /**
         * Common type wrapper part of {@link wrappers}.
         * @interface IWrapper
         * @property {WrapperFromObjectConverter} [fromObject] From object converter
         * @property {WrapperToObjectConverter} [toObject] To object converter
         */ // Custom wrapper for Any
            n[".google.protobuf.Any"] = {
                fromObject: function (e) {// unwrap value type if mapped
                    if (e && e["@type"]) {
                        var t = this.lookup(e["@type"]);/* istanbul ignore else */if (t) {// type_url does not accept leading "."
                            var n = "." === e["@type"].charAt(0) ? e["@type"].substr(1) : e["@type"];// type_url prefix is optional, but path seperator is required
                            return this.create({ type_url: "/" + n, value: t.encode(t.fromObject(e)).finish() })
                        }
                    } return this.fromObject(e)
                }, toObject: function (e, t) {// decode value if requested and unmapped
                    if (t && t.json && e.type_url && e.value) {// Only use fully qualified type name after the last '/'
                        var n = e.type_url.substring(e.type_url.lastIndexOf("/") + 1), i = this.lookup(n); i && (e = i.decode(e.value))
                    }// wrap value if unmapped
                    if (!(e instanceof this.ctor) && e instanceof o) { var r = e.$type.toObject(e, t); return r["@type"] = e.$type.fullName, r } return this.toObject(e, t)
                }
            }
        }, { 21: 21 }], 42: [function (e, t) {/**
         * Constructs a new writer operation instance.
         * @classdesc Scheduled writer operation.
         * @constructor
         * @param {function(*, Uint8Array, number)} fn Function to call
         * @param {number} len Value byte length
         * @param {*} val Value to write
         * @ignore
         */function n(e, t, n) {/**
             * Function to call.
             * @type {function(Uint8Array, number, *)}
             */ /**
             * Value byte length.
             * @type {number}
             */ /**
             * Next operation.
             * @type {Writer.Op|undefined}
             */ /**
             * Value to write.
             * @type {*}
             */this.fn = e, this.len = t, this.next = undefined$1, this.val = n
            }/* istanbul ignore next */function o() { }// eslint-disable-line no-empty-function
/**
         * Constructs a new writer state instance.
         * @classdesc Copied writer state.
         * @memberof Writer
         * @constructor
         * @param {Writer} writer Writer to copy state from
         * @ignore
         */function i(e) {/**
             * Current head.
             * @type {Writer.Op}
             */ /**
             * Current tail.
             * @type {Writer.Op}
             */ /**
             * Current buffer length.
             * @type {number}
             */ /**
             * Next state.
             * @type {State|null}
             */this.head = e.head, this.tail = e.tail, this.len = e.len, this.next = e.states
            }/**
         * Constructs a new writer instance.
         * @classdesc Wire format writer using `Uint8Array` if available, otherwise `Array`.
         * @constructor
         */function r() {/**
             * Current length.
             * @type {number}
             */ /**
             * Operations head.
             * @type {Object}
             */ /**
             * Operations tail
             * @type {Object}
             */ /**
             * Linked forked states.
             * @type {Object|null}
             */this.len = 0, this.head = new n(o, 0, 0), this.tail = this.head, this.states = null
            }/**
         * Creates a new writer.
         * @function
         * @returns {BufferWriter|Writer} A {@link BufferWriter} when Buffers are supported, otherwise a {@link Writer}
         */function s(e, t, n) { t[n] = 255 & e }/**
         * Constructs a new varint writer operation instance.
         * @classdesc Scheduled varint writer operation.
         * @extends Op
         * @constructor
         * @param {number} len Value byte length
         * @param {number} val Value to write
         * @ignore
         */function a(e, t) { this.len = e, this.next = undefined$1, this.val = t } function d(e, t, n) { for (; e.hi;)t[n++] = 128 | 127 & e.lo, e.lo = (e.lo >>> 7 | e.hi << 25) >>> 0, e.hi >>>= 7; for (; 127 < e.lo;)t[n++] = 128 | 127 & e.lo, e.lo >>>= 7; t[n++] = e.lo }/**
         * Writes an unsigned 64 bit value as a varint.
         * @param {Long|number|string} value Value to write
         * @returns {Writer} `this`
         * @throws {TypeError} If `value` is a string and no long library is present.
         */function p(e, t, n) { t[n] = 255 & e, t[n + 1] = 255 & e >>> 8, t[n + 2] = 255 & e >>> 16, t[n + 3] = e >>> 24 }/**
         * Writes an unsigned 32 bit value as fixed 32 bits.
         * @param {number} value Value to write
         * @returns {Writer} `this`
         */t.exports = r; var l = e(39), u = l.LongBits, y = l.base64, f = l.utf8, m; r.create = l.Buffer ? function () { return (r.create = function () { return new m })() }/* istanbul ignore next */ : function () { return new r }, r.alloc = function (e) { return new l.Array(e) }, l.Array !== Array && (r.alloc = l.pool(r.alloc, l.Array.prototype.subarray)), r.prototype._push = function (e, t, o) { return this.tail = this.tail.next = new n(e, t, o), this.len += t, this }, a.prototype = Object.create(n.prototype), a.prototype.fn = function (e, t, n) { for (; 127 < e;)t[n++] = 128 | 127 & e, e >>>= 7; t[n] = e }, r.prototype.uint32 = function (e) { return this.len += (this.tail = this.tail.next = new a(128 > (e >>>= 0) ? 1 : 16384 > e ? 2 : 2097152 > e ? 3 : 268435456 > e ? 4 : 5, e)).len, this }, r.prototype.int32 = function (e) {
                return 0 > e ? this._push(d, 10, u.fromNumber(e))// 10 bytes per spec
                    : this.uint32(e)
            }, r.prototype.sint32 = function (e) { return this.uint32((e << 1 ^ e >> 31) >>> 0) }, r.prototype.uint64 = function (e) { var t = u.from(e); return this._push(d, t.length(), t) }, r.prototype.int64 = r.prototype.uint64, r.prototype.sint64 = function (e) { var t = u.from(e).zzEncode(); return this._push(d, t.length(), t) }, r.prototype.bool = function (e) { return this._push(s, 1, e ? 1 : 0) }, r.prototype.fixed32 = function (e) { return this._push(p, 4, e >>> 0) }, r.prototype.sfixed32 = r.prototype.fixed32, r.prototype.fixed64 = function (e) { var t = u.from(e); return this._push(p, 4, t.lo)._push(p, 4, t.hi) }, r.prototype.sfixed64 = r.prototype.fixed64, r.prototype.float = function (e) { return this._push(l.float.writeFloatLE, 4, e) }, r.prototype.double = function (e) { return this._push(l.float.writeDoubleLE, 8, e) }; var c = l.Array.prototype.set ? function (e, t, n) { t.set(e, n) }/* istanbul ignore next */ : function (e, t, n) { for (var o = 0; o < e.length; ++o)t[n + o] = e[o] };/**
         * Writes a sequence of bytes.
         * @param {Uint8Array|string} value Buffer or base64 encoded string to write
         * @returns {Writer} `this`
         */ /**
         * Writes a string.
         * @param {string} value Value to write
         * @returns {Writer} `this`
         */ /**
         * Forks this writer's state by pushing it to a stack.
         * Calling {@link Writer#reset|reset} or {@link Writer#ldelim|ldelim} resets the writer to the previous state.
         * @returns {Writer} `this`
         */ /**
         * Resets this instance to the last state.
         * @returns {Writer} `this`
         */ /**
         * Resets to the last state and appends the fork state's current write length as a varint followed by its operations.
         * @returns {Writer} `this`
         */ /**
         * Finishes the write operation.
         * @returns {Uint8Array} Finished buffer
         */r.prototype.bytes = function (e) { var t = e.length >>> 0; if (!t) return this._push(s, 1, 0); if (l.isString(e)) { var n = r.alloc(t = y.length(e)); y.decode(e, n, 0), e = n } return this.uint32(t)._push(c, t, e) }, r.prototype.string = function (e) { var t = f.length(e); return t ? this.uint32(t)._push(f.write, t, e) : this._push(s, 1, 0) }, r.prototype.fork = function () { return this.states = new i(this), this.head = this.tail = new n(o, 0, 0), this.len = 0, this }, r.prototype.reset = function () { return this.states ? (this.head = this.states.head, this.tail = this.states.tail, this.len = this.states.len, this.states = this.states.next) : (this.head = this.tail = new n(o, 0, 0), this.len = 0), this }, r.prototype.ldelim = function () { var e = this.head, t = this.tail, n = this.len; return this.reset().uint32(n), n && (this.tail.next = e.next, this.tail = t, this.len += n), this }, r.prototype.finish = function () {
                for (var e = this.head.next,// skip noop
                    t = this.constructor.alloc(this.len), n = 0; e;)e.fn(e.val, t, n), n += e.len, e = e.next;// this.head = this.tail = null;
                return t
            }, r._configure = function (e) { m = e }
        }, { 39: 39 }], 43: [function (e, t) {/**
         * Constructs a new buffer writer instance.
         * @classdesc Wire format writer using node buffers.
         * @extends Writer
         * @constructor
         */function n() { i.call(this) }/**
         * Allocates a buffer of the specified size.
         * @param {number} size Buffer size
         * @returns {Buffer} Buffer
         */function o(e, t, n) {
                40 > e.length ?// plain js is faster for short strings (probably due to redundant assertions)
                r.utf8.write(e, t, n) : t.utf8Write(e, n)
            }/**
         * @override
         */t.exports = n;// extends Writer
            var i = e(42); (n.prototype = Object.create(i.prototype)).constructor = n; var r = e(39), s = r.Buffer; n.alloc = function (e) { return (n.alloc = r._Buffer_allocUnsafe)(e) }; var a = s && s.prototype instanceof Uint8Array && "set" === s.prototype.set.name ? function (e, t, n) { t.set(e, n) }/* istanbul ignore next */ : function (e, t, n) {
                if (e.copy)// Buffer values
                    e.copy(t, n, 0, e.length); else for (var o = 0; o < e.length;)// plain array values
                    t[n++] = e[o++]
            };/**
         * @override
         */n.prototype.bytes = function (e) { r.isString(e) && (e = r._Buffer_from(e, "base64")); var t = e.length >>> 0; return this.uint32(t), t && this._push(a, t, e), this }, n.prototype.string = function (e) { var t = s.byteLength(e); return this.uint32(t), t && this._push(o, t, e), this }
        }, { 39: 39, 42: 42 }]
    }, {}, [19])
}(), proto = rootProto, nested = { Entity: { nested: { Position: { fields: { x: { type: "float", id: 1 }, y: { type: "float", id: 2 }, z: { type: "float", id: 3 } } }, Entity: { fields: { id: { type: "uint64", id: 1 }, position: { type: "Position", id: 2 }, dimension: { type: "int32", id: 3 }, range: { type: "float", id: 4 }, data: { keyType: "string", type: "MValue", id: 5 } } }, DictionaryMValue: { fields: { value: { keyType: "string", type: "MValue", id: 1 } } }, ListMValue: { fields: { value: { rule: "repeated", type: "MValue", id: 1 } } }, MValue: { oneofs: { MValue: { oneof: ["boolValue", "doubleValue", "stringValue", "intValue", "uintValue", "dictionaryValue", "listValue", "entityValue", "nullValue"] } }, fields: { boolValue: { type: "bool", id: 1 }, doubleValue: { type: "double", id: 2 }, stringValue: { type: "string", id: 3 }, intValue: { type: "int64", id: 4 }, uintValue: { type: "uint64", id: 5 }, dictionaryValue: { type: "DictionaryMValue", id: 6 }, listValue: { type: "ListMValue", id: 7 }, entityValue: { type: "uint64", id: 8 }, nullValue: { type: "bool", id: 9 } } }, EntityDataChangeEvent: { fields: { id: { type: "uint64", id: 1 }, key: { type: "string", id: 2 }, value: { type: "MValue", id: 3 } } }, EntityMultipleDataChangeEvent: { fields: { id: { type: "uint64", id: 1 }, data: { keyType: "string", type: "MValue", id: 2 } } }, EntitySendEvent: { fields: { entities: { rule: "repeated", type: "Entity", id: 1 } } }, EntityPositionChangeEvent: { fields: { id: { type: "uint64", id: 1 }, position: { type: "Position", id: 2 } } }, EntityRangeChangeEvent: { fields: { id: { type: "uint64", id: 1 }, range: { type: "float", id: 2 } } }, EntityDimensionChangeEvent: { fields: { id: { type: "uint64", id: 1 }, dimension: { type: "int32", id: 2 } } }, EntityDeleteEvent: { fields: { id: { type: "uint64", id: 1 } } }, EntityCreateEvent: { fields: { entity: { type: "Entity", id: 1 } } }, EntityStreamInEvent: { fields: { entityId: { type: "uint64", id: 1 } } }, EntityStreamOutEvent: { fields: { entityId: { type: "uint64", id: 1 } } }, AuthEvent: { fields: { token: { type: "string", id: 1 } } }, ServerEvent: { oneofs: { Event: { oneof: ["dataChange", "send", "positionChange", "rangeChange", "delete", "create", "multipleDataChange", "dimensionChange"] } }, fields: { dataChange: { type: "EntityDataChangeEvent", id: 1 }, send: { type: "EntitySendEvent", id: 2 }, positionChange: { type: "EntityPositionChangeEvent", id: 3 }, rangeChange: { type: "EntityRangeChangeEvent", id: 4 }, delete: { type: "EntityDeleteEvent", id: 5 }, create: { type: "EntityCreateEvent", id: 6 }, multipleDataChange: { type: "EntityMultipleDataChangeEvent", id: 7 }, dimensionChange: { type: "EntityDimensionChangeEvent", id: 8 } } }, ClientEvent: { oneofs: { Event: { oneof: ["auth", "streamIn", "streamOut"] } }, fields: { auth: { type: "AuthEvent", id: 1 }, streamIn: { type: "EntityStreamInEvent", id: 2 }, streamOut: { type: "EntityStreamOutEvent", id: 3 } } } } } }, bundle = { nested: nested };/*!
 * protobuf.js v6.8.8 (c) 2016, daniel wirtz
 * compiled thu, 19 jul 2018 00:33:25 utc
 * licensed under the bsd-3-clause license
 * see: https://github.com/dcodeio/protobuf.js for details
 */class Proto { constructor() { const e = proto.Root.fromJSON(bundle); this.Entity = e.lookupType("Entity.Entity"), this.Position = e.lookupType("Entity.Position"), this.ClientEvent = e.lookupType("Entity.ClientEvent"), this.ServerEvent = e.lookupType("Entity.ServerEvent"), this.AuthEvent = e.lookupType("Entity.AuthEvent"), this.EntityStreamInEvent = e.lookupType("Entity.EntityStreamInEvent"), this.EntityStreamOutEvent = e.lookupType("Entity.EntityStreamOutEvent") } } var proto$1 = new Proto, streamingWorker = "let maxCoordinate = 50000;\nlet areaSize = 100;\nlet maxAreaIndex = maxCoordinate / areaSize;\n\nonmessage = function (e) {\n    let data = e.data;\n    if (!this.areas) { // Init areas for fast spacing algorithm\n        this.areas = new Array(maxAreaIndex);\n        for (let i = 0; i < maxAreaIndex; i++) {\n            this.areas[i] = new Array(maxAreaIndex);\n            for (let j = 0; j < maxAreaIndex; j++) {\n                this.areas[i][j] = [];\n            }\n        }\n    }\n    //if (!this.entityAreas) {\n    //    this.entityAreas = new Map();\n    //}\n    if (!this.streamedIn) {\n        this.streamedIn = new Map();\n    }\n    if (!this.newStreamIn) {\n        this.newStreamIn = new Set();\n    }\n    if (!this.newStreamOut) {\n        this.newStreamOut = new Set();\n    }\n\n    if (data.reset) {\n        this.streamedIn.clear();\n        this.newStreamIn.clear();\n        this.newStreamOut.clear();\n    }\n\n    if (data.position) {\n        this.position = data.position;\n    }\n    if (data.entities) {\n        // Fill entities in areas\n        for (let i = 0; i < maxAreaIndex; i++) {\n            for (let j = 0; j < maxAreaIndex; j++) {\n                this.areas[i][j] = [];\n            }\n        }\n        for (const entity of data.entities) {\n            addEntityToArea(entity);\n        }\n    }\n    if (data.entityToAdd) {\n        addEntityToArea(data.entityToAdd);\n    }\n    if (data.entityToRemove) {\n        if (this.streamedIn.has(data.entityToRemove.id)) {\n            this.streamedIn.delete(data.entityToRemove.id);\n            postMessage({streamOut: [data.entityToRemove.id]});\n        }\n        removeEntityFromArea(data.entityToRemove);\n    }\n    if (this.position) {\n        start(this.position);\n    }\n};\n\nfunction addEntityToArea(entity) {\n    const [startingYIndex, startingXIndex, stoppingYIndex, stoppingXIndex] = calcStartStopIndex(entity);\n    if (startingYIndex == null || startingXIndex == null || stoppingYIndex == null || stoppingXIndex == null) return;\n    for (let i = startingYIndex; i <= stoppingYIndex; i++) {\n        for (let j = startingXIndex; j <= stoppingXIndex; j++) {\n            this.areas[j][i].push(entity);\n            /*let entityAreasArr;\n            if (!this.entityAreas.has(entity.id)) {\n                entityAreasArr = [];\n                this.entityAreas.set(entity.id, entityAreasArr);\n            } else {\n                entityAreasArr = this.entityAreas.get(entity.id);\n            }\n            entityAreasArr.push([this.areas[j][i], this.areas[j][i].length - 1]);*/\n        }\n    }\n}\n\nfunction calcStartStopIndex(entity) {\n    let posX = offsetPosition(entity.position.x);\n    let posY = offsetPosition(entity.position.y);\n\n    if (posX < 0 || posY < 0 || posX > maxCoordinate || posY > maxCoordinate) return [null, null, null, null];\n\n    let maxX = posX + entity.range;\n    let maxY = posY + entity.range;\n    let minX = posX - entity.range;\n    let minY = posY - entity.range;\n\n    let startingYIndex = Math.floor(minY / areaSize);\n    let startingXIndex = Math.floor(minX / areaSize);\n    let stoppingYIndex = Math.floor(maxY / areaSize);\n    let stoppingXIndex = Math.floor(maxX / areaSize);\n\n    return [startingYIndex, startingXIndex, stoppingYIndex, stoppingXIndex];\n}\n\nfunction removeEntityFromArea(entity) {\n    /*if (this.entityAreas.has(entity.id)) {\n        for (const [areaArr, index] of this.entityAreas.get(entity.id)) {\n            areaArr.splice(index, 1);\n            console.log(\"index to remove\", index);\n            console.log(\"new arr\", areaArr);\n            // Finds entities stored behind that and decrement the stored indexes by one\n            for (let i = index; i < areaArr.length; i++) {\n                if (this.entityAreas.has(areaArr[i].id)) {\n                    const [entityAreaArr, entityIndex] = this.entityAreas.get(areaArr[i].id);\n                    console.log(\"index to update\", entityIndex);\n                    this.entityAreas.set(areaArr[i].id, [entityAreaArr, entityIndex - 1]);\n                }\n            }\n        }\n    }*/\n    const [startingYIndex, startingXIndex, stoppingYIndex, stoppingXIndex] = calcStartStopIndex(entity);\n    if (startingYIndex == null || startingXIndex == null || stoppingYIndex == null || stoppingXIndex == null) return;\n    for (let i = startingYIndex; i <= stoppingYIndex; i++) {\n        for (let j = startingXIndex; j <= stoppingXIndex; j++) {\n            this.areas[j][i] = this.areas[j][i].filter((arrEntity) => arrEntity.id !== entity.id);\n        }\n    }\n}\n\nfunction distance(v1, v2) {\n    const dx = v1.x - v2.x;\n    const dy = v1.y - v2.y;\n    const dz = v1.z - v2.z;\n\n    return Math.sqrt(dx * dx + dy * dy + dz * dz);\n}\n\nfunction offsetPosition(value) {\n    return value + 10000;\n}\n\nfunction start(position) {\n    for (const [id, entity] of this.streamedIn) {\n        if (distance(entity.position, position) > entity.range) {\n            this.newStreamOut.add(entity.id);\n        }\n    }\n\n    for (let key of this.newStreamOut) {\n        this.streamedIn.delete(key);\n    }\n\n    if (this.newStreamOut.size > 0) {\n        postMessage({streamOut: [...this.newStreamOut]});\n        this.newStreamOut.clear();\n    }\n\n    let posX = offsetPosition(position.x);\n    let posY = offsetPosition(position.y);\n\n    if (posX < 0 || posY < 0 || posX > maxCoordinate || posY > maxCoordinate) return;\n\n    let xIndex = Math.floor(posX / areaSize);\n    let yIndex = Math.floor(posY / areaSize);\n\n    let entitiesInArea = this.areas[xIndex][yIndex];\n\n    for (let entity of entitiesInArea) {\n        if (!this.streamedIn.has(entity.id)) {\n            if (distance(entity.position, position) <= entity.range) {\n                this.newStreamIn.add(entity.id);\n                this.streamedIn.set(entity.id, entity)\n            }\n        }\n    }\n\n    if (this.newStreamIn.size > 0) {\n        postMessage({streamIn: [...this.newStreamIn]});\n        this.newStreamIn.clear();\n    }\n}"; class EntityRepository {
    constructor(e) {
    this.websocket = e, window.removeEntity = e => { this.removeEntity(e) }, this.entities = new Map, this.streamedInEntities = new Map; const t = new Blob([streamingWorker], { type: "application/javascript" });//TODO: bundle websocket events
        this.streamingWorker = new Worker(window.URL.createObjectURL(t)), playerPosition.update = e => { this.streamingWorker.postMessage({ position: e }) }, this.streamingWorker.onmessage = t => {
            const n = t.data.streamIn, o = t.data.streamOut; if (n !== void 0) {
                const t = []; for (const o of n) {
                    if (!this.entities.has(o)) { console.log("entity " + o + " not found"); continue } const n = this.entities.get(o);/*
                        const entityBuffer = proto.Entity.encode(proto.Entity.fromObject(entity)).finish();
                        const entityArray = Array.from(entityBuffer);
                        alt.emit("streamInBuffer", entityArray);
                    */this.streamedInEntities.set(o, n), e.sendEvent({ streamIn: proto$1.EntityStreamInEvent.create({ entityId: o }) }), t.push(n)
                } 0 < t.length && alt.emit("streamIn", JSON.stringify(t))
            } if (void 0 !== o) { const t = []; for (const n of o) { if (e.sendEvent({ streamOut: proto$1.EntityStreamOutEvent.create({ entityId: n }) }), !this.streamedInEntities.has(n)) { console.log("entity " + n + " not found"); continue } const o = this.streamedInEntities.get(n); t.push(o), this.streamedInEntities.delete(n) } 0 < t.length && alt.emit("streamOut", JSON.stringify(t)) }
        }
    } isStreamedIn(e) { return this.streamedInEntities.has(e) } getEntities() { return this.entities.values() } setEntities(e) { let t = new Map; for (const n of e) t.set(n.id, n); this.entities = t, this.updateWorker() } addEntity(e) { this.entities.set(e.id, e), this.streamingWorker.postMessage({ position: playerPosition.getPosition(), entityToAdd: EntityRepository.copyEntityWithoutData(e) }) } static copyEntityWithoutData(e) { return { id: e.id, range: e.range, dimension: e.dimension, position: { x: EntityRepository.roundDecimal(e.position.x, 3), y: EntityRepository.roundDecimal(e.position.y, 3), z: EntityRepository.roundDecimal(e.position.z, 3) } } } static roundDecimal(e, t) { let n = Math.pow(10, t); return Math.round(e * n) / n } copyEntitiesWithoutData() { let e = []; for (const [t, n] of this.entities) e.push(EntityRepository.copyEntityWithoutData(n)); return e } removeEntity(e) { if (this.entities.has(e)) { let t = this.entities.get(e); t && (this.entities.delete(e), this.streamingWorker.postMessage({ position: playerPosition.getPosition(), entityToRemove: EntityRepository.copyEntityWithoutData(t) })) } } updateWorker() { this.streamingWorker.postMessage({ position: playerPosition.getPosition(), entities: this.copyEntitiesWithoutData() }) } resetWorker() { this.streamingWorker.postMessage({ reset: !0 }) }
}//TODO: reset all caches ect on disconnect like timeout ect.
class WebSocket$1 {
    constructor(e, t) {
    this.connection = new ReconnectingWebSocket(e), this.connection.binaryType = "arraybuffer", this.entityRepository = new EntityRepository(this), this.connection.onopen = () => { const e = proto$1.AuthEvent.create({ token: t }), n = proto$1.ClientEvent.create({ auth: e }), o = proto$1.ClientEvent.encode(n).finish(); this.connection.send(o) }, this.connection.onerror = e => { console.log("err", e) }, this.connection.onmessage = async t => {//console.log("data", e.data);
        const e = proto$1.ServerEvent.decode(new Uint8Array((await new Response(t.data).arrayBuffer()))), n = proto$1.ServerEvent.toObject(e, { defaults: !0 });//TODO: debug
        //console.log('event ' + JSON.stringify(obj));
        if (null != n.send) { const e = n.send.entities; this.entityRepository.setEntities(e) } else if (null != n.dataChange) {
            const e = n.dataChange; if (this.entityRepository.entities.has(e.id)) {
                const t = this.entityRepository.entities.get(e.id); t.data || (t.data = {}), t.data[e.key] = e.value; const n = this.entityRepository.entities.get(e.id);//console.log("data changed", newEntity.id, newEntity.data);
                this.entityRepository.isStreamedIn(e.id) && (delete e.id, alt.emit("dataChange", JSON.stringify({ entity: n, data: e })))
            }
        } else if (n.positionChange) { const e = n.positionChange; if (this.entityRepository.entities.has(e.id)) { const t = this.entityRepository.entities.get(e.id); t.position = e.position, this.entityRepository.updateWorker() } } else if (n.rangeChange) { const e = n.rangeChange; if (this.entityRepository.entities.has(e.id)) { const t = this.entityRepository.entities.get(e.id); t.range = e.range, this.entityRepository.updateWorker() } } else if (n.delete) { const e = n.delete; e && this.entityRepository.removeEntity(e.id) } else if (n.create) { const e = n.create; e.entity && this.entityRepository.addEntity(e.entity) } else if (n.multipleDataChange) {
            const e = n.multipleDataChange;//console.log(JSON.stringify(multipleDataChange));
            if (this.entityRepository.entities.has(e.id)) {
                const t = this.entityRepository.entities.get(e.id); for (const n in t.data || (t.data = {}), e.data) e.data.hasOwnProperty(n) && (t.data[n] = e.data[n]); const n = this.entityRepository.entities.get(e.id);//console.log("multiple data change", newEntity.id, newEntity.data);
                this.entityRepository.isStreamedIn(e.id) && alt.emit("dataChange", JSON.stringify({ entity: n, data: e.data }))
            }
        } else if (n.dimensionChange) { const e = n.dimensionChange; if (this.entityRepository.entities.has(e.id)) { const t = this.entityRepository.entities.get(e.id); t.dimension = e.dimension, this.entityRepository.updateWorker() } }
    }
    } sendEvent(e) { const t = proto$1.ClientEvent.create(e), n = proto$1.ClientEvent.encode(t).finish(); this.connection.send(n) } deinit() { this.connection.close() }
} !function (e, t) { "function" == typeof define && define.amd ? define([], t) : "undefined" != typeof module && module.exports ? module.exports = t() : e.ReconnectingWebSocket = t() }(window, function () { var n = Math.pow; function t(o, r, s) { function a(e, t) { var n = document.createEvent("CustomEvent"); return n.initCustomEvent(e, !1, !1, t), n } var p = { debug: !1, automaticOpen: !0, reconnectInterval: 1e3, maxReconnectInterval: 3e4, reconnectDecay: 1.5, timeoutInterval: 2e3 }; for (var e in s || (s = {}), p) this[e] = "undefined" == typeof s[e] ? p[e] : s[e]; this.url = o, this.reconnectAttempts = 0, this.readyState = WebSocket.CONNECTING, this.protocol = null; var l = this, u = !1, y = !1, f = document.createElement("div"), m; f.addEventListener("open", function (e) { l.onopen(e) }), f.addEventListener("close", function (e) { l.onclose(e) }), f.addEventListener("connecting", function (e) { l.onconnecting(e) }), f.addEventListener("message", function (e) { l.onmessage(e) }), f.addEventListener("error", function (e) { l.onerror(e) }), this.addEventListener = f.addEventListener.bind(f), this.removeEventListener = f.removeEventListener.bind(f), this.dispatchEvent = f.dispatchEvent.bind(f), this.open = function (o) { m = new WebSocket(l.url, r || []), o || f.dispatchEvent(a("connecting")), (l.debug || t.debugAll) && console.debug("ReconnectingWebSocket", "attempt-connect", l.url); var i = m, s = setTimeout(function () { (l.debug || t.debugAll) && console.debug("ReconnectingWebSocket", "connection-timeout", l.url), y = !0, i.close(), y = !1 }, l.timeoutInterval); m.onopen = function () { clearTimeout(s), (l.debug || t.debugAll) && console.debug("ReconnectingWebSocket", "onopen", l.url), l.protocol = m.protocol, l.readyState = WebSocket.OPEN, l.reconnectAttempts = 0; var e = a("open"); e.isReconnect = o, o = !1, f.dispatchEvent(e) }, m.onclose = function (i) { if (clearTimeout(s), m = null, u) l.readyState = WebSocket.CLOSED, f.dispatchEvent(a("close")); else { l.readyState = WebSocket.CONNECTING; var r = a("connecting"); r.code = i.code, r.reason = i.reason, r.wasClean = i.wasClean, f.dispatchEvent(r), o || y || ((l.debug || t.debugAll) && console.debug("ReconnectingWebSocket", "onclose", l.url), f.dispatchEvent(a("close"))); var s = l.reconnectInterval * n(l.reconnectDecay, l.reconnectAttempts); setTimeout(function () { l.reconnectAttempts++ , l.open(!0) }, s > l.maxReconnectInterval ? l.maxReconnectInterval : s) } }, m.onmessage = function (e) { (l.debug || t.debugAll) && console.debug("ReconnectingWebSocket", "onmessage", l.url, e.data); var n = a("message"); n.data = e.data, f.dispatchEvent(n) }, m.onerror = function (e) { (l.debug || t.debugAll) && console.debug("ReconnectingWebSocket", "onerror", l.url, e), f.dispatchEvent(a("error")) } }, 1 == this.automaticOpen && this.open(!1), this.send = function (e) { if (m) return (l.debug || t.debugAll) && console.debug("ReconnectingWebSocket", "send", l.url, e), m.send(e); throw "INVALID_STATE_ERR : Pausing to reconnect websocket" }, this.close = function (e, t) { "undefined" == typeof e && (e = 1e3), u = !0, m && m.close(e, t) }, this.refresh = function () { m && m.close() } } return t.prototype.onopen = function () { }, t.prototype.onclose = function () { }, t.prototype.onconnecting = function () { }, t.prototype.onmessage = function () { }, t.prototype.onerror = function () { }, t.debugAll = !1, t.CONNECTING = WebSocket.CONNECTING, t.OPEN = WebSocket.OPEN, t.CLOSING = WebSocket.CLOSING, t.CLOSED = WebSocket.CLOSED, t }); class NetworkingEntity { constructor() { alt.on("entitySetup", (e, t) => { const n = e.split("//"), o = n[0] + "//", r = n[1].split(":"); if (2 < r.length) { let e = ""; for (let t = 0; t < r.length - 1; t++)e += r[t], t < r.length - 2 && (e += ":"); this.websocket = new WebSocket$1(o + "[" + e + "]:" + r[r.length - 1], t) } else this.websocket = new WebSocket$1(e, t) }), alt.on("entityDestroy", () => { this.websocket && this.websocket.deinit() }) } } var networkingEntity = new NetworkingEntity; export default networkingEntity;
