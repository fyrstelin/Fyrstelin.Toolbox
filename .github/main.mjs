import bump from './bump.mjs'
import process from 'process'

const [currentVersion, log, stage, buildNumber] = process.argv.slice(2);

const impactRegex = [
  ['breaking', /^💥/],
  ['feature', /^✨/]
]

const impact = (() => {
  const cleaned = log
    .split('\n')
    .map(x => x.trim())
    .filter(Boolean)

  for (const [i, regex] of impactRegex) {
    if (cleaned.some(l => l.match(regex))) {
      return i
    }
  }
  return "none"
})()

console.log(bump(currentVersion, impact, stage, buildNumber))