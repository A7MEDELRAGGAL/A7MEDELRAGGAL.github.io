# ARCHITECTURE.md — Target Reverse-Engineering & Architecture Evidence

**Target System:** Combat Master  
**Shipped Binary Directory:** `D:\SteamLibrary\steamapps\common\Combat Master`  
**Analysis Date:** 2026-10-19  
**Methodology:** Static inspection via PowerShell PE parsing, Unity manifest indexing, and `link.xml` class table decoding (Clean-room static analysis; target was NOT executed).

---

## 1. Concrete Verified Evidence (Observations)

### 1.1 Core Binary Identity
- **File:** `D:\SteamLibrary\steamapps\common\Combat Master\GameAssembly.dll`
- **File Size:** `120,903,240` bytes (`115.30 MB`)
- **SHA-256 Digest:** `53A367B56B3E95E005A9F635E49EC464BB40314CA5E7887C3FE9C2D4C35CEB3A`
- **PE Architecture:** Machine `0x8664` (AMD64 / x86_64), Magic `0x020B` (PE32+), Number of Sections = `12`

### 1.2 PE Section Layout (`GameAssembly.dll`)
Extracted directly by reading PE headers from disk:
```
Section [0]:  name='.text'   VirtAddr=0x00001000 VirtSize=0x0053FDBC RawAddr=0x00000400 RawSize=0x0053FE00 Chars=0x60000020
Section [1]:  name='il2cpp'  VirtAddr=0x00541000 VirtSize=0x03506F58 RawAddr=0x00540200 RawSize=0x03507000 Chars=0x60000020
Section [2]:  name='.rdata'  VirtAddr=0x03A48000 VirtSize=0x00E1112A RawAddr=0x03A47200 RawSize=0x00E11200 Chars=0x40000040
Section [3]:  name='.data'   VirtAddr=0x0485A000 VirtSize=0x0066D098 RawAddr=0x04858400 RawSize=0x003A9800 Chars=0xC0000040
Section [4]:  name='.pdata'  VirtAddr=0x04EC8000 VirtSize=0x00304470 RawAddr=0x04C01C00 RawSize=0x00304600 Chars=0x40000040
Section [5]:  name='.m7pdd0' VirtAddr=0x051CD000 VirtSize=0x0015024C RawAddr=0x04F06200 RawSize=0x00150400 Chars=0x40000040
Section [6]:  name='.6dz023' VirtAddr=0x0531E000 VirtSize=0x010B4400 RawAddr=0x05056600 RawSize=0x010B4400 Chars=0x40000040
Section [7]:  name='.d94mg'  VirtAddr=0x063D3000 VirtSize=0x00304E00 RawAddr=0x0610AA00 RawSize=0x00304E00 Chars=0x60000020
Section [8]:  name='.m7pdd1' VirtAddr=0x066D8000 VirtSize=0x008E026E RawAddr=0x0640F800 RawSize=0x008E0400 Chars=0x60000020
Section [9]:  name='.m7pdd2' VirtAddr=0x06FB9000 VirtSize=0x000008C0 RawAddr=0x06CEFC00 RawSize=0x00000A00 Chars=0xC0000040
Section [10]: name='.m7pdd3' VirtAddr=0x06FBA000 VirtSize=0x0050A08C RawAddr=0x06CF0600 RawSize=0x0050A200 Chars=0x68000060
Section [11]: name='.reloc'  VirtAddr=0x074C5000 VirtSize=0x001504C4 RawAddr=0x071FA800 RawSize=0x00150600 Chars=0x42000040
```

### 1.3 Exact Unity Engine Version Proof
Verified across three distinct disk sources:
1. `D:\SteamLibrary\steamapps\common\Combat Master\UnityPlayer.dll` VersionInfo:
   - `FileVersion`: `6000.3.19.7768564`
   - `ProductVersion`: `6000.3.19f1 (7689f4515d75)`
2. `D:\SteamLibrary\steamapps\common\Combat Master\CombatMaster_Data\StreamingAssets\Bundles\buildlogtep.json` line 3:
   - `"UnityVersion": "6000.3.19f1"`
3. `D:\SteamLibrary\steamapps\common\Combat Master\CombatMaster_Data\StreamingAssets\Bundles\catalog` binary header (offset 0x00):
   - ASCII string: `UnityFS     5.x.x 6000.3.19f1`

### 1.4 Class Inventory Source (564 Classes)
- **File:** `D:\SteamLibrary\steamapps\common\Combat Master\CombatMaster_Data\StreamingAssets\link.xml`
- **Total lines:** 900
- **Total lines matching `CombatMaster.` or `_Code.CombatMaster.`:** exactly `564` lines
- **Sample literal lines directly from file:**
  ```xml
  <type fullname="CombatMaster.Blueprint_24x24" preserve="all" />
  <type fullname="CombatMaster.Battle.Gameplay.Bullet" preserve="all" />
  <type fullname="CombatMaster.Battle.Gameplay.Weapon.ShootingWeaponView" preserve="all" />
  <type fullname="CombatMaster.Battle.Gameplay.Actors.ActorJumpPad" preserve="all" />
  <type fullname="CombatMaster.Battle.Gameplay.Actors.ActorZipLine" preserve="all" />
  <type fullname="CombatMaster.View.Battle.ShootWeapon" preserve="all" />
  <type fullname="CombatMaster.View.Battle.WeaponSway" preserve="all" />
  ```

### 1.5 Networking Subsystem Proof (Photon Bolt & udpkit)
Literal assembly names from `D:\SteamLibrary\steamapps\common\Combat Master\CombatMaster_Data\ScriptingAssemblies.json`:
- `PhotonBolt.dll`
- `PhotonChat.dll`
- `PhotonRealtime.dll`
- `udpkit.platform.dotnet.dll`
- `udpkit.platform.photon.dll`
- `udpkit.dll`
- `udpkit.platform.null.dll`
- `udpkit.common.dll`
- `bolt.dll`
- `bolt.user.dll`

Literal runtime hook from `D:\SteamLibrary\steamapps\common\Combat Master\CombatMaster_Data\RuntimeInitializeOnLoads.json`:
- `assemblyName`: `PhotonRealtime`, `nameSpace`: `Photon.Realtime`, `className`: `ConnectionHandler`, `methodName`: `StaticReset`

### 1.6 Blueprint Collections Proof (47 Themed Bundles)
Literal lines from `D:\SteamLibrary\steamapps\common\Combat Master\CombatMaster_Data\StreamingAssets\Bundles\StandaloneWindows64.manifest`:
- Bundles `n0` through `n46` present verbatim in manifest:
  - `src/blueprints/n0_no_collection`
  - `src/blueprints/n1_laserstrike_collection`
  - `...`
  - `src/blueprints/n45_sig_mg338_streetfighter_collection`
  - `src/blueprints/n46_stalker_collection`

---

## 2. Inferences (Tagged & Bounded)

- **Inference 1 (Custom Section Roles):**
  - Sections `.m7pdd0`, `.6dz023`, `.d94mg`, `.m7pdd1`, `.m7pdd2`, `.m7pdd3` are non-standard MSVC / Unity PE sections.
  - *Basis:* Standard Unity IL2CPP produces only `.text`, `il2cpp`, `.rdata`, `.data`, `.pdata`, and `.reloc`. The presence of additional executable and data sections totaling ~34 MB indicates a packer or code protection wrapper.

- **Inference 2 (Absence of standard metadata):**
  - Standard IL2CPP requires type and method indices normally stored in `global-metadata.dat`. Since `global-metadata.dat` is absent from disk, the metadata must reside elsewhere or be resolved through custom runtime routines.

---

## 3. Explicit Unknowns

- **Exact Runtime Metadata Resolution:** UNKNOWN. Whether the metadata is decrypted into heap memory, mapped from a section, or resolved on demand cannot be verified without executing the binary in a debugger, which was not performed.
- **Internal Method Implementations & Variable Names:** UNKNOWN. Because no CIL bytecode exists (native x64 machine code only) and metadata is not exposed statically, private local variables, exact mathematical formulas, and internal algorithm details remain unknown.
- **Exact Physics & Ballistic Tunables:** UNKNOWN. Movement acceleration curves, friction coefficients, slide duration thresholds, and weapon damage numbers are embedded inside compiled binary data or encrypted asset bundles, and are not directly readable from plain text.
