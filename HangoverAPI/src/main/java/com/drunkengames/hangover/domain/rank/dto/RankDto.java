package com.drunkengames.hangover.domain.rank.dto;

import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@AllArgsConstructor
@NoArgsConstructor
@Getter
@Setter
public class RankDto {
    private Long id;

    private String userNickname;

    private Integer finalMoney;

    private Integer finalDay;
}
