//NOTE: it's clearly a bad sign that eslint is complaining about the complexity in this file.
/* eslint-disable complexity */
/* eslint-env browser */
/* eslint-disable no-alert, no-console */
import React from 'react';
import viaTheme from 'es-components-via-theme';
import { AuthenticatedScreen, Screen } from '@mktp/screen';
import { ThemeProvider } from 'styled-components';
import { Control, ActionButton, Textbox, Label, Icon, Fieldset, Spinner, Notification, Message } from 'es-components';
import 'whatwg-fetch';
import fetchJsonp from 'fetch-jsonp';

function App() {
  let [isbn, setIsbn] = React.useState('');

  /* NOTE: this mode/setMode mechanism is not meant to represent best-practices */
  let [mode, setMode] = React.useState('search');

  let [failureMessage, setFailureMessage] = React.useState();

  let [publicationDetails, setPublicationDetails] = React.useState();

  let [publicationCopyId, setPublicationCopyId] = React.useState();

  async function lookupIsbn() {
    setFailureMessage(undefined);
    setPublicationDetails(undefined);
    setMode('searching');

    var googlebookresultPromise = fetchJsonp(`https://www.googleapis.com/books/v1/volumes?q=isbn:${isbn}`, {
      timeout: 30000, // 30 seconds
    });
    /* NOTE: there is no user input sanitization nor urlencoding on the isbn like there should be */
    var localResultPromise = fetch(`/circulation/api/collection/publication/${isbn}`, { method: 'HEAD' });
    var googlebookresult = await googlebookresultPromise;
    var localResult = await localResultPromise;

    let details = null;

    if (googlebookresult.ok) {
      var result = await googlebookresult.json();
      if (result.totalItems) {
        details = {
          foundInGoogle: true,
          title: result.items[0].volumeInfo.title,
          authors: `${result.items[0].volumeInfo.authors}`,
          coverImageUrl: result.items[0].volumeInfo.imageLinks.thumbnail
        }
      }
    }
    if(details === null) {
      details = {
        foundInGoogle: false,
        title: '',
        authors: '',
        coverImageUrl: null
      }
    }
    setPublicationDetails(details);

    if (localResult.ok) {
      // then this is a publication we already know, so we only need to let the user confirm it and add a copy.
      setMode('confirm');
    } else {
      // then this is a new publication, so we should let the user confirm and/or update the details, add it, and then add a copy of it.
      setMode('input');
    }
  }

  async function addPublicationAndCopy() {
    const toPost = {
      isbn: isbn,
      title: publicationDetails.title,
      authors: publicationDetails.authors,
      coverImageUrl: publicationDetails.coverImageUrl
    }
    let addPublicationResult = await fetch(`/circulation/api/collection/publication`, {method: 'POST', body: JSON.stringify(toPost), headers: {'content-type': 'application/json'}});
    if(addPublicationResult.ok) {
      await addCopy('input');
    }
    else {
      setFailureMessage('Add failed. You might try again.');
    }
  }

  async function addCopy(fromMode = 'confirm') {
    setMode('searching');
    let addCopyResult = await fetch(`/circulation/api/collection/publication/${isbn}/copies`, {method: 'POST'});
    if(addCopyResult.ok) {
      let result = await addCopyResult.json();
      setPublicationCopyId(result);
      setMode('added');
    }
    else {
      setMode(fromMode)
      setFailureMessage('Add failed. You might try again.');
    }
  }

  const buttonStyle = {
    margin: '10px 10px 10px 0'
  };

  const iconStyle = {
    position: 'relative',
    top: '-2px'
  };

  return (
    <ThemeProvider theme={viaTheme}>
      <AuthenticatedScreen>
        <Screen.Header />
        <Screen.Content style={{padding:'10px'}}>
          {mode === 'search' &&
            <Fieldset legendContent="Lookup the ISBN">
              <Control>
                <Label htmlFor="isbn">ISBN</Label>
                <Textbox id="isbn" value={isbn} onChange={event => setIsbn(event.target.value)} />
              </Control>
              <div style={{display:'flex'}}>
                <ActionButton styleType="success" style={buttonStyle} onClick={lookupIsbn}><Icon name="search" style={iconStyle} /> Lookup</ActionButton>
              </div>
            </Fieldset>
          }
          {mode === 'searching' &&
            <Spinner title="Searching" height="65px" width="65px" />
          }
          {
            //NOTE: there are certainly better ways to factor all this JSX code and eliminate lots of repetition

            mode === 'confirm' &&
            <Fieldset legendContent="Is this the right publication?">
              {failureMessage &&
              <Notification type="danger">
                <div>{failureMessage}</div>
              </Notification>
              }
              {publicationDetails ?
              (
                <div style={{display:'flex'}}>
                  {publicationDetails.coverImageUrl &&
                    <div style={{padding: '15px'}}>
                      <img src={publicationDetails.coverImageUrl} alt="publication cover" />
                    </div>
                  }
                  <div style={{flexGrow:1}}>
                    <Control orientation="stacked">
                      <Label htmlFor="publicationIsbn">ISBN</Label>
                      <div>{isbn}</div>
                      </Control>
                    <Control orientation="stacked">
                      <Label htmlFor="publicationTitle">Title</Label>
                      <Textbox id="publicationTitle" readOnly defaultValue={publicationDetails.title} />
                    </Control>
                    <Control orientation="stacked">
                      <Label htmlFor="publicationAuthors">Authors</Label>
                      <Textbox id="publicationAuthors" readOnly defaultValue={publicationDetails.authors} />
                    </Control>
                  </div>
                </div>
              ) : (
                <Notification type="info">
                  <div>At least one copy of this publication is already in circulation, but the details were not found.</div>
                </Notification>
              )
              }
              <div style={{display:'flex'}}>
                <ActionButton styleType="success" style={buttonStyle} onClick={addCopy}><Icon name="add" tyle={iconStyle} /> Add a copy of this publication</ActionButton>
                <ActionButton styleType="info" style={buttonStyle} onClick={() => setMode('search')}>Nope. Go back</ActionButton>
              </div>
            </Fieldset>
          }
          {mode === 'input' &&
            <Fieldset legendContent="Provide publication details">
              {failureMessage &&
              <Notification type="danger">
                <div>{failureMessage}</div>
              </Notification>
              }
              { (!publicationDetails.foundInGoogle) &&
                <Notification type="info">
                <div>Details for this ISBN weren&apos;t found.</div>
              </Notification>
              }

              <div style={{display:'flex'}}>
                {publicationDetails.coverImageUrl &&
                  <div style={{padding: '15px'}}>
                    <img src={publicationDetails.coverImageUrl} alt="publication cover" />
                  </div>
                }
                <div style={{flexGrow:1}}>
                  <Control orientation="stacked">
                    <Label htmlFor="publicationIsbn">ISBN</Label>
                    <div>{isbn}</div>
                    </Control>
                  <Control orientation="stacked">
                    <Label htmlFor="publicationTitle">Title</Label>
                    <Textbox id="publicationTitle" value={publicationDetails.title} onChange={e => setPublicationDetails({...publicationDetails, title: e.target.value})} />
                  </Control>
                  <Control orientation="stacked">
                    <Label htmlFor="publicationAuthors">Authors</Label>
                    <Textbox id="publicationAuthors" value={publicationDetails.authors} onChange={e => setPublicationDetails({...publicationDetails, authors: e.target.value})}/>
                  </Control>
                </div>
              </div>
              <div style={{display:'flex'}}>
                <ActionButton styleType="success" style={buttonStyle} onClick={addPublicationAndCopy}><Icon name="add" tyle={iconStyle} /> Add this new publication</ActionButton>
                <ActionButton styleType="info" style={buttonStyle} onClick={() => setMode('search')}>Go back</ActionButton>
              </div>
            </Fieldset>
          }
          {mode === 'added' &&
            <>
            <Notification type="success">
              <Message
                emphasizedText="Success"
                text={`CopyId: ${publicationCopyId}`}
              />
            </Notification>
            <div style={{display:'flex'}}>
                <ActionButton styleType="info" style={buttonStyle} onClick={() => { setIsbn(''); setMode('search');}}>Add another</ActionButton>
              </div>
            </>
          }
        </Screen.Content>
        <Screen.Footer />
      </AuthenticatedScreen>
    </ThemeProvider>
  );
}

export default App;
