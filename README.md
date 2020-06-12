# README

.NET targeting latest framework (4.7) and AnyCPU. Should work against x86 and x64 processes. Doesn't need special privileges like `SeDebugPrivilege`.

Motivation: Someone at work built a similar program, and I was like "how hard could that be"? So even though it is fairly useless, here it is. 

## Config

Follow this format:

```
---
- title: SomeTitle
  marker: SimpleString
  regex: MoreComplexRegex
  url_decode: true | false
```

The config file should be an array of maps with at least a title, marker, and regex. 

It should be saved as `inputs.yaml` in the pwd or used on the command line. 

## Running

Or like this: `memsearch`

Or like this: `memsearch <process filter regex>`

Run like this: `memsearch <process filter regex> <base64 config>`

## Performance and stability

On my laptop (Win 10 / i7-6700HQ with 32GB of RAM), it can run around 55MB/s with one regex pattern (the username= one). 

I ran it over 100 times and it didn't crash once. It may fail in the extern calls to kernel32, but it ~~won't~~shouldn't crash. 
