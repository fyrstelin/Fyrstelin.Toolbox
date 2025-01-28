function version(major, minor, patch, prerelease, build) {
  const core = [major, minor, patch].join('.');

  return [
    core,
    prerelease && ("-" + prerelease),
    build && ("." + build),
  ]
    .filter(Boolean)
    .join('')
}

function appendVersion(stage, prereleaseVersion) {
  if (!stage) return stage;
  if (stage === "alpha")
    return stage
  const v = prereleaseVersion ? parseInt(prereleaseVersion) + 1 : 0
  return [stage, v].join('.')
}

/**
 * @param {string} currentVersion 
 * @param {"breaking"|"feature"|"none"} impact 
 * @param {"alpha"|"beta"|"rc"} [stage]
 * @param {string} [buildNumber]
 */
export default function bump(currentVersion, impact, stage, buildNumber) {
  const [coreAndPreRelease] = currentVersion.split('+')
  const [core, prereleaseWithVersion] = coreAndPreRelease.split('-')
  const [prerelease, prereleaseVersion] = (prereleaseWithVersion || "").split('.')

  const [major, minor, patch] = core.split('.').map(x => parseInt(x));

  const stageWithVersion = appendVersion(stage, prereleaseVersion)

  if (stage && prerelease) {
    switch (impact) {
      case 'breaking': return version(major + Math.min(minor + patch, 1), 0, 0, minor ? appendVersion(stage) : stageWithVersion, buildNumber)
      case 'feature': return version(major, minor + Math.min(patch, 1), 0, patch ? appendVersion(stage) : stageWithVersion, buildNumber)
      case 'none': return version(major, minor, patch, stageWithVersion, buildNumber)
    }  
  }

  if (prerelease) {
    return version(major, minor, patch)
  }

  switch (impact) {
    case 'breaking': return version(major + 1, 0, 0, stageWithVersion, buildNumber)
    case 'feature': return version(major, minor + 1, 0, stageWithVersion, buildNumber)
    case 'none': return version(major, minor, patch + 1, stageWithVersion, buildNumber)
  }
}