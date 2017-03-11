# encodeDecodeBase64

Simple WPF program to encode/decode text files in base64

Before run you need add remote server to TrustedHosts:
Set-item wsman:localhost\client\trustedhosts -value *

To check your TrustedHosts use this:
WinRM get winrm/config/client