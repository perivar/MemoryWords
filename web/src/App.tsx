import { useState, useEffect } from 'react'
import { parseDigits, toMnemonic } from './utils/mnemonic'
import { FindWords } from './utils/wordFinder'
import { DigitsWords } from './utils/DigitsWords'

function App() {
  const [digits, setDigits] = useState('')
  const [words, setWords] = useState('')
  const [mnemonicResult, setMnemonicResult] = useState('')
  const [wordGroups, setWordGroups] = useState<DigitsWords[]>([])
  const [dictionary, setDictionary] = useState<string[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isProcessing, setIsProcessing] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const DEFAULT_DIGITS = '31415926535897932384' // First 20 digits of π
  const DEFAULT_WORDS = 'motorhotell penkjole milf byggeboomen omveier'

  // Load dictionary when component mounts
  useEffect(() => {
    const loadDictionary = async () => {
      try {
        setIsLoading(true)
        const response = await fetch('/dict/nsf2023.txt');
        if (!response.ok) {
          throw new Error(`Failed to load dictionary (${response.status} ${response.statusText})`)
        }
        const text = await response.text()
        const words = text.split(/\r?\n/).filter(Boolean)
        if (words.length === 0) {
          throw new Error('Dictionary file is empty')
        }
        setDictionary(words)
      } catch (err) {
        console.error('Dictionary loading error:', err)
        setError(err instanceof Error ? err.message : 'Failed to load dictionary')
      } finally {
        setIsLoading(false)
      }
    }

    loadDictionary()
  }, [])

  const handleDigitSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    // Use default digits if none are entered
    const digitsToUse = digits.trim() || DEFAULT_DIGITS
    const digitArray = digitsToUse.split('').map(Number)

    setIsProcessing(true)
    setWordGroups([]) // Clear previous results

    try {
      await FindWords(
        dictionary,
        digitArray,
        (foundWords) => {
          setWordGroups(prev => [...prev, foundWords])
        }
      )
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error processing words')
    } finally {
      setIsProcessing(false)
    }
  }

  const handleWordSubmit = (e: React.FormEvent) => {
    e.preventDefault()

    // Split the input into individual words and process each one
    const wordList = (words.trim() || DEFAULT_WORDS).split(/\s+/)
    const results = wordList.map(word => {
      const foundDigits = parseDigits(word)
      const mnemonic = toMnemonic(word)
      return `${word}\n${foundDigits}\n${mnemonic}`
    })

    setMnemonicResult(results.join('\n\n'))
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="bg-white shadow rounded-lg p-6">
          <h2 className="text-xl font-semibold text-red-600 mb-2">Error</h2>
          <p className="text-gray-700">{error}</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-100 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-2xl mx-auto">
        <h1 className="text-3xl font-bold text-center text-gray-900 mb-4">
          Memory Words Converter
        </h1>
        <div className="bg-white shadow rounded-lg p-6 mb-8">
          <p className="text-gray-600 mb-4">
            The Mnemonic Major System converts numbers to consonant sounds that can form memorable words. This Norwegian adaptation helps remember long numbers like π (3.1415926535897932384) through word associations.
          </p>
          <p className="text-gray-600 mb-4">
            For example: The number {'53138552'} becomes:
            <br/>
            {'LAM'} (5=L, 3=M) - {'DUM'} (1=D, 3=M) - {'FLY'} (8=F, 5=L) - {'LYN'} (5=L, 2=N)
          </p>
          <p className="text-gray-600">
            See the reference table below for the complete Norwegian mapping system.
          </p>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center p-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            <span className="ml-3 text-gray-600">Loading dictionary...</span>
          </div>
        ) : (
          <>
            {/* Words for Number Section */}
            <div className="bg-white shadow rounded-lg p-6 mb-6">
              <h2 className="text-xl font-semibold mb-2">Words for Number</h2>
              <p className="text-sm text-gray-600 mb-4">
                Enter a number to find Norwegian words that can help you remember it using the mapping table above.
                For example: {'53'} can be {'LAM'} (L=5, M=3).
              </p>
              <form onSubmit={handleDigitSubmit} className="mb-6">
                <div className="mb-4">
                  <label htmlFor="digits" className="block text-sm font-medium text-gray-700 mb-1">
                    Enter Number
                  </label>
                  <input
                    type="text"
                    id="digits"
                    value={digits}
                    onChange={(e) => setDigits(e.target.value)}
                    className="input-field w-full px-3 py-2 border border-gray-300 rounded-md"
                    placeholder={`Enter a number (default: ${DEFAULT_DIGITS})`}
                  />
                </div>
                <button
                  type="submit"
                  disabled={isProcessing}
                  className={`w-full bg-blue-600 text-white px-4 py-2 rounded-md ${
                    isProcessing ? 'opacity-50 cursor-not-allowed' : 'hover:bg-blue-700'
                  }`}
                >
                  {isProcessing ? 'Processing...' : 'Show Words'}
                </button>
              </form>

              {(isProcessing || wordGroups.length > 0) && (
                <div className="mt-6 space-y-6">
                  {isProcessing && (
                    <div className="flex items-center justify-center py-4">
                      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                      <span className="ml-3 text-gray-600">Finding words...</span>
                    </div>
                  )}

                  {wordGroups.map((group, groupIndex) => (
                    <div key={groupIndex} className="p-4 bg-gray-50 rounded-md">
                      <h3 className="text-lg font-medium text-gray-900 mb-3">
                        Found digits {group.Digits.join(',')}:
                      </h3>
                      <div className="pl-4">
                        <div className="columns-3 gap-4">
                          {group.WordCandidates.map((word, wordIndex) => (
                            <div key={wordIndex} className="text-sm text-gray-600 break-inside-avoid">
                              {word}
                            </div>
                          ))}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* Number for Words Section */}
            <div className="bg-white shadow rounded-lg p-6">
              <h2 className="text-xl font-semibold mb-2">Number for Words</h2>
              <p className="text-sm text-gray-600 mb-4">
                Enter Norwegian words to see their digit representations using the mapping table above.
                For example: {'LAM'} becomes {'53'} (L=5, M=3).
              </p>
              <form onSubmit={handleWordSubmit} className="mb-6">
                <div className="mb-4">
                  <label htmlFor="words" className="block text-sm font-medium text-gray-700 mb-1">
                    Enter Word(s)
                  </label>
                  <input
                    type="text"
                    id="words"
                    value={words}
                    onChange={(e) => setWords(e.target.value)}
                    className="input-field w-full px-3 py-2 border border-gray-300 rounded-md"
                    placeholder={`Enter word(s) (default: ${DEFAULT_WORDS})`}
                  />
                </div>
                <button
                  type="submit"
                  className="w-full bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700"
                >
                  Convert Words to Digits
                </button>
              </form>

              {mnemonicResult && (
                <div className="mt-6 p-4 bg-gray-50 rounded-md">
                  <h3 className="text-lg font-medium text-gray-900 mb-2">
                    Result
                  </h3>
                  <pre className="whitespace-pre font-mono text-sm text-gray-600">
                    {mnemonicResult}
                  </pre>
                </div>
              )}
            </div>
          </>
        )}

        <div className="bg-white shadow rounded-lg p-6 mt-8">
          <h2 className="text-xl font-semibold mb-2">Norwegian Mapping Table</h2>
          <table className="min-w-full bg-white border border-gray-300">
            <thead>
              <tr>
                <th className="py-2 px-4 border-b text-sm">Digit</th>
                <th className="py-2 px-4 border-b text-sm">Speech Sounds (IPA)</th>
                <th className="py-2 px-4 border-b text-sm">Associated Letters & Examples</th>
                <th className="py-2 px-4 border-b text-sm">Notes</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td className="py-2 px-4 border-b text-sm">0</td>
                <td className="py-2 px-4 border-b text-sm">/s/, /z/</td>
                <td className="py-2 px-4 border-b text-sm">S som i Sirkel, Z som i Zalo</td>
                <td className="py-2 px-4 border-b text-sm">Sirkel eller 0 på engelsk ZERO</td>
              </tr>
              <tr>
                <td className="py-2 px-4 border-b text-sm">1</td>
                <td className="py-2 px-4 border-b text-sm">/t/, /d/</td>
                <td className="py-2 px-4 border-b text-sm">T som i Tal, D som i Dal</td>
                <td className="py-2 px-4 border-b text-sm">t og d har én nedstrek</td>
              </tr>
              <tr>
                <td className="py-2 px-4 border-b text-sm">2</td>
                <td className="py-2 px-4 border-b text-sm">/n/</td>
                <td className="py-2 px-4 border-b text-sm">N som i Nei</td>
                <td className="py-2 px-4 border-b text-sm">n har to nedstreker</td>
              </tr>
              <tr>
                <td className="py-2 px-4 border-b text-sm">3</td>
                <td className="py-2 px-4 border-b text-sm">/m/</td>
                <td className="py-2 px-4 border-b text-sm">M som i Meg</td>
                <td className="py-2 px-4 border-b text-sm">m har tre nedstreker</td>
              </tr>
              <tr>
                <td className="py-2 px-4 border-b text-sm">4</td>
                <td className="py-2 px-4 border-b text-sm">/r/</td>
                <td className="py-2 px-4 border-b text-sm">R som i Rein</td>
                <td className="py-2 px-4 border-b text-sm">tenk på fiRe, eller R som i rein, fire bein</td>
              </tr>
              <tr>
                <td className="py-2 px-4 border-b text-sm">5</td>
                <td className="py-2 px-4 border-b text-sm">/l/</td>
                <td className="py-2 px-4 border-b text-sm">L som i Liv</td>
                <td className="py-2 px-4 border-b text-sm">Romertallet L er 50</td>
              </tr>
              <tr>
                <td className="py-2 px-4 border-b text-sm">6</td>
                <td className="py-2 px-4 border-b text-sm">/ʃ/, /tʃ/</td>
                <td className="py-2 px-4 border-b text-sm">SJ som i Sjø, SKJ som i Skje, KJ som i Kjede, TJ som i Tjue</td>
                <td className="py-2 px-4 border-b text-sm">J har en kurve nederst slik som 6 har</td>
              </tr>
              <tr>
                <td className="py-2 px-4 border-b text-sm">7</td>
                <td className="py-2 px-4 border-b text-sm">/k/, /g/</td>
                <td className="py-2 px-4 border-b text-sm">K som i Kul, G som i Gul</td>
                <td className="py-2 px-4 border-b text-sm">K inneholder to 7-tall</td>
              </tr>
              <tr>
                <td className="py-2 px-4 border-b text-sm">8</td>
                <td className="py-2 px-4 border-b text-sm">/f/, /v/</td>
                <td className="py-2 px-4 border-b text-sm">F som i Fisk, V som i Visk</td>
                <td className="py-2 px-4 border-b text-sm">Jeg assosierer V med V8. F lyder som V når den uttales. F ligner på 8</td>
              </tr>
              <tr>
                <td className="py-2 px-4 border-b text-sm">9</td>
                <td className="py-2 px-4 border-b text-sm">/p/, /b/</td>
                <td className="py-2 px-4 border-b text-sm">P som i Pil, B som i Bil</td>
                <td className="py-2 px-4 border-b text-sm">9 rotert 180 grader ser ut som b. 9 speilvendt ser ut som P. P og B lyder likt når de uttales.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}

export default App