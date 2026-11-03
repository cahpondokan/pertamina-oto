// Scanner choice and installation identity are independent: Play updates must
// retain the original package/name while using the same scanner as the trial.
const isMLKitTrial = bool.fromEnvironment('OTO_MLKIT_TRIAL');
const googleScannerEnabled =
    isMLKitTrial || bool.fromEnvironment('OTO_GOOGLE_SCANNER');
const applicationName = isMLKitTrial ? 'OTO Scan Uji' : 'OTO Scan';
