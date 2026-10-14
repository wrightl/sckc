import { getTestBed } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';

// The specs are shallow CLI-generated tests that don't import child components or Material modules,
// so keep the pre-v21 behaviour of not failing on unknown elements/properties.
getTestBed().initTestEnvironment(BrowserTestingModule, platformBrowserTesting(), {
  errorOnUnknownElements: false,
  errorOnUnknownProperties: false,
});
