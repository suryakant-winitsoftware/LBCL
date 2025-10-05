// Test YearMonth format calculation

const date = new Date()
const isoString = date.toISOString()

console.log('Current Date:', date)
console.log('ISO String:', isoString)
console.log('')

// OLD (WRONG) - YYYYMM format
const oldFormat = parseInt(isoString.slice(0, 7).replace('-', ''))
console.log('OLD Format (YYYYMM):', oldFormat)
console.log('  slice(0, 7):', isoString.slice(0, 7))
console.log('  Expected: 202510 (6 digits)')
console.log('')

// NEW (CORRECT) - YYMM format
const newFormat = parseInt(isoString.slice(2, 7).replace('-', ''))
console.log('NEW Format (YYMM):', newFormat)
console.log('  slice(2, 7):', isoString.slice(2, 7))
console.log('  Expected: 2510 (4 digits)')
console.log('')

console.log('Partition expects YYMM format:', newFormat)
