# REST GPIO - Final Quick Reference ✅

## 🎯 Endpoint Format (CORRECTED)

```
GET GPIO Outputs:  /api/devices/device;{dirno}/gpos
GET GPIO Inputs:   /api/devices/device;{dirno}/gpis

With Query Parameter:
                   /api/devices/device;{dirno}/gpos?dirno={dirno}
                   /api/devices/device;{dirno}/gpis?dirno={dirno}

Example (dirno=1020):
                   /api/devices/device;1020/gpos
                   /api/devices/device;1020/gpis
```

## ✨ Key Points

- ✅ **Semicolon required**: `device;{dirno}` (not slash)
- ✅ **OAuth2**: Bearer token added automatically
- ✅ **Nested arrays**: [[{...}]] → [{...}] (auto-flattened)
- ✅ **Auto-discovery**: Tests 4 endpoint formats
- ✅ **Type field**: relay, gpi, or gpo

## 🧪 Expected Response

```json
[
  [
    {
      "id": "relay1",
      "state": "low",
      "type": "relay"
    }
  ]
]
```

## 📊 All Issues Fixed

| Issue | Status |
|-------|--------|
| 401 Unauthorized | ✅ OAuth2 |
| 403 Forbidden | ✅ Fixed format |
| JSON Deserialization | ✅ Array handling |
| Nested Arrays | ✅ Flattening |
| Endpoint Format | ✅ Corrected |

## 🚀 Ready to Use!

Just run your application - everything is automatic! ✅

**Build**: Successful ✅
**Status**: Ready ✅
