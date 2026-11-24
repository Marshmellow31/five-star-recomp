# Status inputs

The progress image on the wiki's Status page (`docs/wiki/progress.svg`) and the short chart in the README
(`docs/assets/progress-summary.svg`) are drawn from these files. CI fails when an image does not match them.

- `manual.json`: hand-kept counts for every area (no game data). Update it when merged work changes a number.
- `status.json`: the collected status, kept so areas without a new source keep their last value.

```
fivestar progress collect --manual docs/status/manual.json docs/status/status.json
fivestar progress docs/status/status.json docs/wiki/progress.svg
fivestar progress summary docs/status/status.json docs/assets/progress-summary.svg
```

`collect` also takes `--shaders <results.json>` (a translate run) and `--dump <dir>` (texture and archive counts
measured from a dump) to replace those areas with measured values.
