import * as React from "react";
import { Provider, Flex, Text, Button, Header, Input, InputProps, DropdownProps, Dropdown } from "@fluentui/react-northstar";
import { useState, useEffect, useRef } from "react";
import { useTeams } from "msteams-react-base-component";
import { app, pages } from "@microsoft/teams-js";
export interface IConfigMathTabState {
    mathOperator ?: string;
    operandA: number;
    operandB: number;
    result: string;
  }

/**
 * Implementation of ConfigMathTab configuration page
 */
export const ConfigMathTabConfig = () => {

    const [{ inTeams, theme, context }] = useTeams({});
    const [mathOperator, setMathOperator] = useState<string>();
    const entityId = useRef("");
    const [mathTabState, setMathTabState] = useState<IConfigMathTabState>({ mathOperator: "add" } as IConfigMathTabState);


    const onSaveHandler = (saveEvent: pages.config.SaveEvent) => {
        const host = "https://" + window.location.host;
        pages.config.setConfig({
            contentUrl: host + "/configMathTab/?name={loginHint}&tenant={tid}&group={groupId}&theme={theme}",
            websiteUrl: host + "/configMathTab/?name={loginHint}&tenant={tid}&group={groupId}&theme={theme}",
            suggestedDisplayName: "ConfigMathTab",
            removeUrl: host + "/configMathTab/remove.html?theme={theme}",
            entityId: entityId.current
        }).then(() => {
            saveEvent.notifySuccess();
        });
    };

    useEffect(() => {
        if (context) {
            setMathOperator(context.page.id.replace("MathPage", ""));
            entityId.current = context.page.id;
            pages.config.registerOnSaveHandler(onSaveHandler);
            pages.config.registerOnSaveHandler(onSaveHandler);
            pages.config.setValidityState(true);
            app.notifySuccess();
        }
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [context]);

    const handleOnChangedOperandA = (data?: InputProps): void => {
        if (data && !isNaN(Number(data.value))) {
          setMathTabState(state => ({
            ...state,
            operandA: data.value
          } as IConfigMathTabState));
        }
      };
      
      const handleOnChangedOperandB = (data?: InputProps): void => {
        if (data && !isNaN(Number(data.value))) {
          setMathTabState(state => ({
            ...state,
            operandB: data.value
          } as IConfigMathTabState));
        }
      };
      
      const handleOperandChange = (): void => {
        let stringResult: string = "n/a";
      
        if (mathTabState) {
          if (!isNaN(Number(mathTabState.operandA)) && !isNaN(Number(mathTabState.operandB))) {
            switch (mathTabState.mathOperator) {
              case "add":
                stringResult = (Number(mathTabState.operandA) + Number(mathTabState.operandB)).toString();
                break;
              case "subtract":
                stringResult = (Number(mathTabState.operandA) - Number(mathTabState.operandB)).toString();
                break;
              case "multiply":
                stringResult = (Number(mathTabState.operandA) * Number(mathTabState.operandB)).toString();
                break;
              case "divide":
                stringResult = (Number(mathTabState.operandA) / Number(mathTabState.operandB)).toString();
                break;
              default:
                stringResult = "n/a";
                break;
            }
          }
        }
        setMathTabState(state => ({
          ...state,
          result: stringResult
        } as IConfigMathTabState));
      };

      return (
        <Provider theme={theme}>
          <Flex column gap="gap.smaller">
            <Header>This is your tab</Header>
            <Text content="Enter the values to calculate" size="medium"></Text>
      
            <Flex gap="gap.smaller">
              <Flex.Item>
                <Flex gap="gap.smaller">
                  <Flex.Item>
                    <Input autoFocus
                      value={mathTabState.operandA}
                      onChange={(e, data) => handleOnChangedOperandA(data)}></Input>
                  </Flex.Item>
                  <Text content={mathTabState.mathOperator}></Text>
                  <Flex.Item>
                    <Input value={mathTabState.operandB}
                      onChange={(e, data) => handleOnChangedOperandB(data)}></Input>
                  </Flex.Item>
                </Flex>
              </Flex.Item>
              <Button content="Calculate" primary
                onClick={handleOperandChange}></Button>
              <Text content={mathTabState.result}></Text>
            </Flex>
            <Text content="(C) Copyright Contoso" size="smallest"></Text>
          </Flex>
        </Provider>
      );
};
