"""
c03e lab backend.

INTENTIONALLY VULNERABLE -- for isolated, authorized security-training use
only (practicing with Fiddler / Burp Suite / Process Hacker against the
matching c03e.exe thick client). Do not expose this outside a lab network.

Run:
    pip install -r requirements.txt
    python app.py
Listens on http://127.0.0.1:5000 (plain HTTP on purpose -- nothing for the
client to validate, trivial to intercept).
"""

import hashlib
import pickle
import base64
from flask import Flask, request, jsonify

app = Flask(__name__)

# --- "database" -------------------------------------------------------
USERS = {
    1: {"user_id": 1, "username": "jdoe",   "password": "Summer2024!", "full_name": "John Doe",
        "role": "User",  "email": "jdoe@c03e.local",
        "notes": "Q3 budget draft attached in shared drive."},
    2: {"user_id": 2, "username": "asmith", "password": "Passw0rd123", "full_name": "Alice Smith",
        "role": "User",  "email": "asmith@c03e.local",
        "notes": "Payroll adjustment pending approval."},
    3: {"user_id": 3, "username": "admin",  "password": "AdminPass!2024", "full_name": "System Administrator",
        "role": "Admin", "email": "admin@c03e.local",
        "notes": "Master keys rotate quarterly -- see vault.dat backup."},
}

USERNAME_TO_ID = {u["username"]: uid for uid, u in USERS.items()}

# VULN (server-side, backs client VULN #13): tokens are deterministic --
# MD5(username + static salt) -- never expire, aren't bound to an IP/device,
# and are identical every time the same user logs in. Capture one in
# Fiddler/Burp and it is valid forever; you can also precompute a valid
# token for any known username without ever logging in.
STATIC_SALT = "c03e_salt_2024"


def make_token(username: str) -> str:
    return hashlib.md5((username + STATIC_SALT).encode()).hexdigest()


@app.route("/api/login", methods=["POST"])
def login():
    data = request.get_json(force=True, silent=True) or {}
    username = data.get("username", "")
    password = data.get("password", "")

    uid = USERNAME_TO_ID.get(username)
    user = USERS.get(uid) if uid else None

    if user and user["password"] == password:
        return jsonify({
            "authenticated": True,
            "user_id": user["user_id"],
            "full_name": user["full_name"],
            "role": user["role"],
            "email": user["email"],
            "token": make_token(username),
        })

    # NOTE: the response shape is identical whether creds are wrong or the
    # user doesn't exist, EXCEPT for the "authenticated" boolean -- which
    # the client trusts blindly (see c03e ApiClient/LoginForm VULN #5).
    return jsonify({"authenticated": False}), 200


@app.route("/api/users/<int:user_id>", methods=["GET"])
def get_user(user_id):
    # VULN #8 (IDOR): only checks that SOME session token was presented in
    # the X-Session-Token header -- never checks that the token actually
    # belongs to `user_id`. Any authenticated user can enumerate
    # /api/users/1, /api/users/2, /api/users/3 ... and read everyone else's
    # profile, including the admin's notes field.
    token = request.headers.get("X-Session-Token", "")
    if not token:
        return jsonify({"error": "missing token"}), 401

    user = USERS.get(user_id)
    if not user:
        return jsonify({"error": "not found"}), 404

    return jsonify(user)


@app.route("/api/users/<int:user_id>", methods=["PUT"])
def update_user(user_id):
    # Same missing-ownership-check problem as GET, plus it lets the caller
    # set arbitrary fields including "role" -> IDOR-driven privilege
    # escalation if a learner also patches the client to send role changes.
    token = request.headers.get("X-Session-Token", "")
    if not token:
        return jsonify({"error": "missing token"}), 401

    user = USERS.get(user_id)
    if not user:
        return jsonify({"error": "not found"}), 404

    updates = request.get_json(force=True, silent=True) or {}
    user.update({k: v for k, v in updates.items() if k in ("full_name", "email", "notes", "role")})
    return jsonify(user)


@app.route("/api/config", methods=["GET"])
def config():
    # No auth required at all -- plaintext backend config exposed to
    # anyone who can reach the endpoint (great target to find by just
    # browsing around in Fiddler's captured traffic).
    return jsonify({
        "db_connection_string": "Server=corp-sql01.c03e.local;Database=c03e_prod;User Id=c03e_app;Password=Autumn2024!;",
        "backup_api_key": "c03e-svc-8271-BACKUP-ADMIN-KEY",
        "environment": "production",
    })


@app.route("/api/whoami-backdoor", methods=["GET"])
def whoami_backdoor():
    # Reachable with the hardcoded backup key from c03e ApiClient.cs
    # (VULN #1) instead of a real session -- demonstrates the impact of a
    # hardcoded service-account credential.
    api_key = request.headers.get("X-Api-Key", "")
    if api_key == "c03e-svc-8271-BACKUP-ADMIN-KEY":
        return jsonify({"authenticated_as": "svc_backup", "role": "Admin", "via": "hardcoded api key"})
    return jsonify({"error": "unauthorized"}), 401


@app.route("/api/deserialize", methods=["POST"])
def deserialize_endpoint():
    # BONUS server-side insecure deserialization (Python pickle), separate
    # from the client-side BinaryFormatter one (VULN #6) -- mirrors the
    # same class of bug on the network side for tools like Burp to probe.
    # A base64-encoded pickle payload built with a gadget (e.g. os.system
    # via __reduce__) achieves RCE the moment this endpoint deserializes it.
    blob = request.get_json(force=True, silent=True) or {}
    raw = blob.get("data", "")
    try:
        obj = pickle.loads(base64.b64decode(raw))
        return jsonify({"result": str(obj)})
    except Exception as e:
        return jsonify({"error": str(e)}), 400


@app.route("/update/latest", methods=["GET"])
def update_latest():
    # Serves a "binary" with no signature and no hash pinning -- the client
    # UpdateService.cs downloads and EXECUTES whatever comes back here.
    # Swap this response in Fiddler/Burp (or on a rogue Wi-Fi AP against a
    # real HTTP deployment) to plant an arbitrary payload.
    fake_exe = b"MZ" + b"\x90" * 62 + b"This is a placeholder, not a real PE."
    return fake_exe, 200, {"Content-Type": "application/octet-stream"}


if __name__ == "__main__":
    app.run(host="127.0.0.1", port=5000, debug=True)
