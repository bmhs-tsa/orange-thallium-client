# Generate the shared entropy for the Windows DPAPI
# This file is automatically executed each time before the full-trust launcher is built

# Instantiate a new RNGCryptoServiceProvider
$provider = [System.Security.Cryptography.RNGCryptoServiceProvider]::Create()

# Generate 16 crypto-safe random bytes
$bytes = [System.Byte[]]::CreateInstance([System.Byte], 16)
$provider.GetBytes($bytes)

# Save bytes to file
[IO.File]::WriteAllBytes("$PSScriptRoot/../entropy.key", $bytes)